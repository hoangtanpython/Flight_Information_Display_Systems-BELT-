<template>
    <div>
        <div v-if="currentFlight">
        <img alt="Vuelogo" :src="`/img/fullscreen/${currentFlight.lineCode}_1920x1080.png?timestamp=${new Date().getTime()}`" @error="handleImageError(currentFlight)" style="overflow: hidden;"/>
        </div>
        <div v-else>
          <img  alt="Vuelogo" :src="`/img/fullscreen/AHT_1920x1080.png?timestamp=${new Date().getTime()}`" />
        </div>
    </div>
</template>
  
  <script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { parse } from 'date-fns';
  const urlFlight = `http://172.17.18.12:8085/api/CheckinCounter`;
  const timeCheckFlight = 30;
  const timeLoadFlight = 100;
  
  definePageMeta({
  layout: 'checkinlayout' // Chỉ định layout custom
  })
  interface Flight {id: string;scheduledDate: string;schedule: string;estimated: string;actual: string;lineCode: string;
    flight: string;city: string;gate: string;remark: string;status: string;rowFrom: string;rowTo: string;
    checkInCounters: string;counterStart: string;counterEnd: string;gateStart: string;gateEnd: string;
  }
  const flights = ref<Flight[]>([]);
  const currentFlight = ref<Flight | null>(null);
  const fetchFlights = async () => {
    const headers = new Headers();
    headers.append('Cache-Control', 'no-cache, no-store, must-revalidate');
    headers.append('Pragma', 'no-cache');
    headers.append('Expires', '0');
    fetch(urlFlight)
    .then(response => response.json())
    .then(data => {
      console.log("fetch ok !");
      flights.value = data;
      checkCurrentFlight(); // Kiểm tra chuyến bay hiện tại
    })
    .catch(error => {
      console.error(error);
    });
  };
  
  const parseDate = (dateString: string): Date => {
  // Parse the date using the custom format 'dd/MM/yyyy h:mm:ss a'
  return parse(dateString, 'dd/MM/yyyy h:mm:ss a', new Date());
  };

  const checkCurrentFlight = () => {
    const now = new Date();
    currentFlight.value = flights.value.find((flight) => {
      const counterStart = new Date(parseDate(flight.counterStart));
      const counterEnd = new Date(parseDate(flight.counterEnd));
      return now >= counterStart && now <= counterEnd;
    }) || null;
    console.log(now);
  };
  
  const handleImageError = (item: Flight) => {
    console.log(item.lineCode);
         item.lineCode = 'AHT'; // Thay đổi tên hình thay thế tại đây
         console.log(item.lineCode);
        };


  onMounted(async () => {
    await fetchFlights(); // Gọi API cập nhật dữ liệu flights trước
    setInterval(checkCurrentFlight, timeCheckFlight*1000); // Cập nhật chuyến bay mỗi phút
    setInterval(fetchFlights, timeLoadFlight*1000); // Gọi API cập nhật flights mỗi 5 phút (300000ms)
  });

  </script>
  
  <style scoped>

  </style>
  