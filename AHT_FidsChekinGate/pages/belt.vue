<template>
  <div class="belt-screen">
    <div v-if="flight">
      <div class="top-info">
        <p class="destination">{{ flight.destination }}</p>
        <h1 class="flightNumber">{{ flight.flightNumber }}</h1>
      </div>
      <img :src="logoSrc" @error="onLogoError" alt="Airline Logo" class="logo" />
      <div class="info-container">
        <div class="left-info">
          <p class="title">{{ language === 'vi' ? 'GIỜ DỰ KIẾN:' : 'ESTIMATED TIME:' }}</p>
          <p class="scheduledTime">{{ formattedTime(flight.displayTime) }}</p>
        </div>
        <div class="right-info">
          <p class="title">{{ language === 'vi' ? 'TRẠNG THÁI:' : 'STATUS:' }}</p>
          <p class="status" :class="statusClass">{{ translatedStatus }}</p>
        </div>
      </div>
      <img src="/img/logo/logoAHT.png" alt="logo nhà ga quốc tế đà nẵng" class="logoAHT" />
    </div>
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
import { ref, computed, watchEffect, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import * as signalR from '@microsoft/signalr'
const config = useRuntimeConfig()

interface ArrivalFlight {
  flightNumber: string
  destination: string
  displayTime: string | null  
  endTime: string | null      
  status: string              
  airline?: string | null
  belt: string | null     
}

/** ==============================
 *  Nguồn & trạng thái màn hình
 *  ============================== */
const route = useRoute()
// Belt lấy từ ?belt=1 
const belt = computed(() => String(route.query.belt ?? '1'))

const flight = ref<ArrivalFlight | null>(null)
const language = ref<'vi' | 'en'>('vi')

// Logo theo 2 ký tự đầu của flightNumber 
const airlineCode = computed(() =>
  (flight.value?.flightNumber ?? '').slice(0, 2).toUpperCase()
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
  // Nếu PNG lỗi, thử JPG một lần; sau đó rơi về placeholder : file ảnh có 2 định dạng PNG và JPG
  if (logoSrc.value.endsWith('.png') && !triedJpgOnce) {
    triedJpgOnce = true
    logoSrc.value = logoSrc.value.replace('.png', '.jpg')
  } else {
    logoSrc.value = '/img/1920x480/placeholder_1920x480.png'
  }
}

/** ==============================
 *  Helpers hiển thị
 *  ============================== */
function formattedTime(raw?: string | null) {
  if (!raw) return ''
  // Backend trả 'YYYY-MM-DD HH:mm:ss'
  const d = new Date(raw.replace(' ', 'T'))
  if (isNaN(+d)) {
    const m = raw.match(/\b(\d{2}):(\d{2})\b/)
    return m ? `${m[1]}:${m[2]}` : ''
  }
  const hh = d.getHours().toString().padStart(2, '0')
  const mm = d.getMinutes().toString().padStart(2, '0')
  return `${hh}:${mm}`
}

// 1) Nhóm chuẩn hoá rất gọn (dựa trên 3 chuỗi EN từ API)
function normalizeFromEn(raw?: string | null) {
  const s = (raw || '').trim().toLowerCase()
  if (s.includes('bags delivered')) return 'delivered'
  if (s.includes('baggage arriving')) return 'claiming'
  if (s.includes('waiting for baggage')) return 'waiting'
  // fallback (nếu API trong tương lai khác đôi chút)
  if (s === 'arrived') return 'claiming'
  if (s.startsWith('bags deliver')) return 'delivered'
  return 'waiting'
}

const translatedStatus = computed(() => {
  const norm = normalizeFromEn(flight.value?.status)
  if (language.value === 'vi') {
    if (norm === 'delivered') return 'Đã trả hành lý'
    if (norm === 'claiming')  return 'Đang trả hành lý'
    return 'Đang đợi hành lý'
  }

  if (norm === 'delivered') return 'Bags Delivered'
  if (norm === 'claiming')  return 'Baggage Arriving'
  return 'Waiting for Baggage'
})

const statusClass = computed(() => {
  const norm = normalizeFromEn(flight.value?.status)
  if (norm === 'delivered') return 'status-done'      
  if (norm === 'claiming')  return 'status-claiming'  
  return 'status-waiting'                              
})


/** ==============================
 *  Data + SignalR
 *  ============================== */
let connection: signalR.HubConnection | null = null
let refreshTimer: number | undefined
let langTimer: number | undefined

async function fetchForBelt() {
  try {
    // Endpoint gợi ý từ backend mới: GET /api/arrivals/belt/{belt}
    const res = await fetch(`${config.public.apiBase}/arrivals/belt/${belt.value}`, { cache: 'no-store' })
    const data = await res.json() as ArrivalFlight | null

    
    if (!data) {
      flight.value = null
      return
    }

    
    const end = data.endTime ? new Date(data.endTime.replace(' ', 'T')) : null
    if (end && Date.now() > +end) {
      flight.value = null
      return
    }

    flight.value = data
  } catch {
    flight.value = null
  }
}

onMounted(async () => {
  await fetchForBelt()

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${config.public.hubBase}/flight?belt=${belt.value}`)
    .withAutomaticReconnect()
    .build()

  connection.on('UpdateBeltFlight', (data: ArrivalFlight | null) => {
    if (!data) {
      flight.value = null
      return
    }
    const end = data.endTime ? new Date(data.endTime.replace(' ', 'T')) : null
    flight.value = end && Date.now() > +end ? null : data
  })

  try {
    await connection.start()
  } catch (e) {
    console.error('Hub start error:', e)
  }

  refreshTimer = window.setInterval(fetchForBelt, 30000)
  langTimer = window.setInterval(() => {
    language.value = language.value === 'vi' ? 'en' : 'vi'
  }, 5000)
})

onUnmounted(async () => {
  if (refreshTimer) clearInterval(refreshTimer)
  if (langTimer) clearInterval(langTimer)
  try { await connection?.stop() } catch {}
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

.left-info { text-align: left; }

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
  height: 100%; }
.fullscreen-image { 
  width: 100%; 
  height: 100%; 
  object-fit: cover; }

.title {
  font-weight: 700;
  font-size: 3vw;
  color: #36c0c7;
}

.left-info p, .right-info p { 
  margin: 0 0 80px 0; }

.left-info, .right-info {
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
  width: 100%; }


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
