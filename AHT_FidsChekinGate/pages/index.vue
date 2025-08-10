<template>
  <Gate v-if="gate" />
  <Checkin v-if="checkin" />
  <SortingArr v-if="sortingArr" />
  <SortingDep v-if="sortingDep" />
  <Arr85 v-if="arr85" />
  <Dep v-if="dep" />
  <DepLed v-if="depLed" />
  <Arr v-if="arr" />
  <Ahtlogo v-if="ahtlogo" />
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Gate from './gate.vue'
import Checkin from './checkin.vue'
import SortingArr from './SortingArr.vue'
import SortingDep from './SortingDep.vue'
import Arr85 from './Arrival85.vue'
import Dep from './Departures.vue'
import DepLed from './DeparturesLED.vue'
import Arr from './Arrival.vue'
import Ahtlogo from './ahtlogo.vue'

import * as signalR from '@microsoft/signalr'

const gate = ref(false)
const checkin = ref(false)
const sortingArr = ref(false)
const sortingDep = ref(false)
const arr85 = ref(false)
const dep = ref(false)
const depLed = ref(false)
const arr = ref(false)
const ahtlogo = ref(false)

const ulrCheckDevice = 'https://localhost:7248/api/FidsLocation'
// const ulrCheckDevice = 'https://localhost:7079/api/FidsLocation'

const offMode = () => {
  gate.value = false
  checkin.value = false
  ahtlogo.value = false
  sortingArr.value = false
  sortingDep.value = false
  arr85.value = false
  dep.value = false
  arr.value = false
  depLed.value = false
}

const applyLayoutByDescription = (desc: string) => {
  const description = desc.trim()
  switch (description) {
    case 'Checkin':
      offMode()
      checkin.value = true
      break
    case 'Gate':
      offMode()
      gate.value = true
      break
    case 'SortingDep':
      offMode()
      sortingDep.value = true
      break
    case 'SortingArr':
      offMode()
      sortingArr.value = true
      break
    case 'Arr85':
      offMode()
      arr85.value = true
      break
    case 'Dep':
      offMode()
      dep.value = true
      break
    case 'DepLed':
      offMode()
      depLed.value = true
      break
    case 'Arr':
      offMode()
      arr.value = true
      break
    default:
      offMode()
      ahtlogo.value = true
      break
  }
}

const fetchFlights = () => {
  fetch(ulrCheckDevice)
    .then((response) => response.json())
    .then((data) => {
      console.log('fetch ok !', data)
      if (data.length > 0) {
        applyLayoutByDescription(data[0].description)
      } else {
        offMode()
        ahtlogo.value = true
      }
    })
    .catch((error) => {
      console.error(error)
    })
}

onMounted(async () => {
  // Tạm bỏ gọi fetch API vì đang lỗi timeout
  // fetchFlights()

  // ✅ Gọi layout trực tiếp để test
  applyLayoutByDescription("Checkin")

  // Kết nối SignalR
  const connection = new signalR.HubConnectionBuilder()
    .withUrl('https://localhost:7248/hub/flight')
    .withAutomaticReconnect()
    .build()

  connection.start()
    .then(() => {
      console.log('✅ SignalR connected')
    })
    .catch((err) => {
      console.error('❌ SignalR connection error:', err)
    })

  connection.on("SendToClient", (data) => {
    console.log("📡 Dữ liệu realtime từ backend:", data)
    if (data && data.location) {
      applyLayoutByDescription(data.location)
    }
  })
})

</script>

<style scoped>
/* Style tuỳ bạn thêm sau */
</style>
