<template>
  <div class="belt-screen">
    <div v-if="flight">
      <!-- Dòng trên cùng: Điểm đến & số hiệu -->
      <div class="top-info">
        <p class="destination">{{ flight.Destination }}</p>
        <h1 class="flightNumber">{{ flight.FlightNumber }}</h1>
      </div>

      <!-- Logo hãng (1920x480) -->
      <img :src="logoSrc" @error="onLogoError" alt="Airline Logo" class="logo" />

      <!-- Khối thông tin dưới -->
      <div class="info-container">
        <!-- Cột trái: giờ dự kiến -->
        <div class="left-info">
          <p class="title">{{ language === 'vi' ? 'GIỜ DỰ KIẾN:' : 'ESTIMATED TIME:' }}</p>
          <p class="scheduledTime">{{ formattedTime(flight.ScheduledTime) }}</p>
        </div>

        <!-- Cột phải: trạng thái -->
        <div class="right-info">
          <p class="title">{{ language === 'vi' ? 'TRẠNG THÁI:' : 'STATUS:' }}</p>
          <p class="status" :class="statusClass">{{ translatedStatus }}</p>
        </div>
      </div>

      <!-- Logo AHT cuối màn -->
      <img src="/img/logo/logoAHT.png" alt="logo nhà ga quốc tế đà nẵng" class="logoAHT" />
    </div>

    <!-- Không có chuyến: hiển thị ảnh nền full-screen -->
    <div v-else class="no-flight">
      <img
        alt="AHT fullscreen"
        :src="`/img/fullscreen/AHT_1920x1080.png?timestamp=${Date.now()}`"
        class="fullscreen-image"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watchEffect, onMounted, onUnmounted, defineProps } from 'vue'
import { useRoute } from 'vue-router'
import * as signalR from '@microsoft/signalr'

function normalizeFlight(raw: any) {
  if (!raw) return null
  // map về schema mà template đang dùng
  return {
    FlightNumber: String(raw.FlightNumber ?? raw.flightNumber ?? ''),
    Destination:  String(raw.Destination  ?? raw.destination  ?? ''),
    ScheduledTime:String(raw.ScheduledTime?? raw.scheduledTime?? ''),
    Status:       String(raw.Status       ?? raw.status       ?? ''),
    Airline:      raw.Airline ?? raw.airline ?? undefined,
    Carousel:     String(raw.Carousel ?? raw.carousel ?? '')
  }
}

// Cho phép truyền prop 'carousel' (VD: B1, B2, ...)
const props = defineProps<{ carousel?: string }>()

interface ArrivalFlight {
  FlightNumber: string
  Destination: string
  ScheduledTime: string
  Status: string
  Airline?: string
  Carousel: string
}

const flight = ref<ArrivalFlight | null>(null)
const language = ref<'vi' | 'en'>('vi')

// Xác định belt từ prop hoặc query (?carousel=B1)
const route = useRoute()
const belt = computed(() => props.carousel || (route.query.carousel as string) || 'B1')

/* ================= Logo hãng ================= */
const airlineCode = computed(() =>
  (flight.value?.FlightNumber ?? '').slice(0, 2).toUpperCase()
)

const logoSrc = ref('')
let triedJpgOnce = false

watchEffect(() => {
  triedJpgOnce = false
  logoSrc.value = airlineCode.value
    ? `/img/1920x480/${airlineCode.value}_1920x480.png`
    : ''
})

function onLogoError() {
  // Nếu PNG lỗi, thử JPG một lần; sau đó rơi về placeholder
  if (logoSrc.value.endsWith('.png') && !triedJpgOnce) {
    triedJpgOnce = true
    logoSrc.value = logoSrc.value.replace('.png', '.jpg')
  } else {
    logoSrc.value = '/img/1920x480/placeholder_1920x480.png'
  }
}

/* ================= Helpers hiển thị ================= */
function formattedTime(raw: string) {
  if (!raw) return ''
  // Backend trả 'YYYY-MM-DD HH:mm:ss' => đổi thành ISO hợp lệ
  const d = new Date(raw.replace(' ', 'T'))
  if (isNaN(+d)) {
    // Fallback: cắt "HH:mm" từ chuỗi gốc nếu parse thất bại
    const m = raw.match(/\b(\d{2}):(\d{2})\b/)
    return m ? `${m[1]}:${m[2]}` : ''
  }
  const hh = d.getHours().toString().padStart(2, '0')
  const mm = d.getMinutes().toString().padStart(2, '0')
  return `${hh}:${mm}`
}

const translatedStatus = computed(() => {
  const s = (flight.value?.Status || '').toLowerCase()
  if (language.value === 'vi') {
    if (s.includes('waiting')) return 'Đang đợi hành lý'
    if (s.includes('claim')) return 'Đang trả hành lý'
    if (s.includes('deliver')) return 'Đã trả hành lý'
  }
  // Mặc định: giữ nguyên string server
  return flight.value?.Status || ''
})

const statusClass = computed(() => {
  const s = (flight.value?.Status || '').toLowerCase()
  if (s.includes('waiting')) return 'status-waiting'
  if (s.includes('claim')) return 'status-claiming'
  if (s.includes('deliver')) return 'status-done'
  return ''
})

/* ================= Data + SignalR ================= */
let connection: signalR.HubConnection | null = null
let refreshTimer: number | undefined
let langTimer: number | undefined

async function fetchAuto() {
  try {
    const res = await fetch(
      `https://localhost:7248/api/arrivals/belt-auto?belt=${belt.value}`,
      { cache: 'no-store' }
    )
    const data = await res.json() // object hoặc null
    console.log('REST belt-auto:', data)        // tạm log để bạn nhìn payload
    flight.value = normalizeFlight(data)
  } catch {
    flight.value = null
  }
}

onMounted(async () => {
  await fetchAuto()

  // Kết nối đúng hub + tham số belt để join group belt-<B1>
  connection = new signalR.HubConnectionBuilder()
    .withUrl(`https://localhost:7248/dashboardHub?belt=${belt.value}`)
    .withAutomaticReconnect()
    .build()

  connection.on('UpdateBeltFlight', (data: any) => {
  console.log('HUB UpdateBeltFlight:', data)
  flight.value = normalizeFlight(data)
  })

  try {
    await connection.start()
  } catch (e) {
    console.error('Hub start error:', e)
  }

  // Refresh định kỳ + đổi ngôn ngữ
  refreshTimer = window.setInterval(fetchAuto, 30000)
  langTimer = window.setInterval(() => {
    language.value = language.value === 'vi' ? 'en' : 'vi'
  }, 5000)
})

onUnmounted(async () => {
  if (refreshTimer) clearInterval(refreshTimer)
  if (langTimer) clearInterval(langTimer)
  try {
    await connection?.stop()
  } catch (e) {
    console.error('Lỗi ngắt SignalR:', e)
  }
})
</script>

<style scoped>
.belt-screen {
  width: 1920px;
  height: 1080px;
  background-color: #283b92;
  text-align: center;
  overflow: hidden;
}

.logo {
  width: 100%;
  height: 100%;
  padding-bottom: 25px;
}

.info-container {
  display: flex;
  justify-content: space-between;
  padding-top: 25px;
  color: white;
}

.left-info {
  text-align: left;
}

.destination {
  color: #fff;
  font-weight: 700;
  font-size: 8vw;
}

.scheduledTime {
  color: #fff;
  font-weight: 700;
  font-size: 5vw;
}

.flightNumber {
  color: #fff;
  font-weight: 700;
  font-size: 8vw;
}

.status {
  color: #fff;
  font-weight: 700;
  font-size: 5vw;
}

.no-flight {
  width: 100%;
  height: 100%;
}

.fullscreen-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.title {
  font-weight: 700;
  font-size: 3vw;
  color: #36c0c7;
}

.left-info p {
  margin: 0;
  margin-bottom: 80px;
}

.right-info p {
  margin: 0;
  margin-bottom: 80px;
}

.left-info,
.right-info {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1;
}

.top-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 50px;
}

.top-info .destination {
  font-size: 8vw;
  font-weight: 700;
  color: #fff;
  text-align: left;
}

.top-info .flightNumber {
  font-size: 8vw;
  font-weight: 700;
  color: #fff;
  text-align: right;
}

.logoAHT {
  width: 100%;
}

.status-waiting {
  color: yellow;
}
.status-claiming {
  color: #00ff00;
}
.status-done {
  color: red;
}
</style>
