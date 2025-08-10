<template>
    <HomeAppHeaderDep />
    <section class="content">
      <div class="row">
              <div class="card-body pad table-responsive" style="width: 100vw">
                      <table class="table table-hover" id="tblWorkOrderDone">
                          <thead class="theadheader" style="background-color: #36c0c7;height: 6vh;">
                              <tr>
                                  <th style="width: 10vw;">STD</th>
                                  <th style="width: 15vw;">AIRLINE</th>
                                  <th style="width: 10vw;">FLIGHT</th>
                                  <th style="width: 21vw; "><spam style="float: left; padding-left: 2.4vw;">DESTINATION</spam></th>
                                  <th style="width: 11vw;">COUNTER</th>
                                  <th style="width: 10vw;">NEW TIME</th>
                                  <th style="width: 10vw;">CHECK-IN</th>
                                  <th style="width: 16vw;">STATUS</th>
                              </tr>
                          </thead>
                          <tbody>
                            <template v-for="(flight, index) in pagedGroups" :key="flight.flight + '-' + index">
                              <!-- Nếu là dòng thông báo ngày -->
                              <tr v-if="flight.isDaybreak" class="sizerow notice-row" style="height: 13.7vh;">
                                <td colspan="8" style="vertical-align: middle;">
                                  <div class="notice-content">{{ formatDateToFlightMessage(flight.date) }}</div>
                                </td>
                              </tr>

                              <!-- Nếu là chuyến bay -->
                              <tr v-else-if="flight.flight" class="sizerow">
                                <td class="midal">{{ formatTime(flight.schedule) }}</td>
                                <td class="midal">
                                  <img alt="Vuelogo"
                                      :src="`/img/logo/${flight.lineCode}_240x38.png?timestamp=${new Date().getTime()}`"
                                      @error="handleImageError(flight)" />
                                </td>
                                <td class="midal">{{ flight.flight }}</td>
                                <td class="midal1">{{ getFullCityName(flight.city) }}</td>
                                <td class="midal">{{ flight.checkInCounters }}</td>
                                <td class="midal">{{ formatTime(flight.actual || flight.estimated) }}</td>
                                <td class="midal">{{ formatTime(flight.counterStart) }}</td>
                                <td class="midal" :class="getRemarkClass(flight)">
                                  {{ getRemark(flight) }}
                                </td>
                              </tr>
                            </template>
                          </tbody>
                      </table>
                  </div>
          </div >
    </section>
    <HomeAppFooter /> 
  </template>
  
  <script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { parse, format } from 'date-fns'
  import { watch, reactive, toRefs } from 'vue';
  import * as signalR from "@microsoft/signalr";


  // const urlCountries = 'https://localhost:7079/api/Countries';
  // const urlConfig = 'https://localhost:7079/api/FidsInformation';
  // const urlbase = 'https://localhost:7079/';
  // const urlHub = 'https://localhost:7248/dashboardHub';

  const urlCountries = 'http://172.17.18.12:8085/api/Countries';
  const urlConfig = 'http://172.17.18.12:8085/api/FidsInformation';
  const urlbase = 'http://172.17.18.12:8085';
  const urlHub = 'https://localhost:7248/dashboardHub';

const countries = reactive({
  cityMap: [
      {codeAirport: "BHY",nameAirport: "Beihai",countries: "Trung Quốc"},
      {codeAirport: "ICN",nameAirport: "Incheon",countries: "Hàn Quốc"}
  ]});

interface Flight {
  id: string;scheduledDate: string;schedule: string;estimated: string; actual: string; counterStart: string ; counterEnd: string;
  lineCode: string;flight: string;city: string; gate: string;remark: string; checkInCounters: string; codeShare: string
};
const flights = ref<Flight[]>([]);

const pagedGroupsData = ref<any[][]>([])
const currentPage = ref(0)
let pageSize = 10
let maxPages = 2
let pageInterval = 3000
let reloadInterval = 180000 // 3 phút

async function loadFlights(rollOn: number, rollOff: number) {

  flights.value = await refetchDataArr(rollOn, rollOff);
  //console.log(flights.value);
  const allFlights = groupFlightsByDayWithBreak(flights.value)
  pagedGroupsData.value = paginateFlights(allFlights)
  if (currentPage.value >= pagedGroupsData.value.length) {
    currentPage.value = 0;
  }
}
  
const loadCityMap = async () => {
    fetch(urlCountries) 
          .then(response => response.json())
          .then(data => {
            countries.cityMap = data;
          })
          .catch(error => {
          console.error(error);
          });
};

const refetchDataArr = async (rollOn: number,  rollOff: number): Promise<Flight[]> =>
  {
  const config = useRuntimeConfig()
  const { data, error } = await useFetch<Flight[]>('/api/FidsDepartures', {
    baseURL: urlbase,
    method: 'GET',
    query: { rollOn, rollOff}
  });
   //flights.value = data.value ?? [];
  if (error.value) {
    console.error('❌ API Error:', error.value)
    return []
  }
  return data.value ?? []
}


function groupFlightsByDayWithBreak(flights: Flight[]) {
  //console.log(flights[0].scheduledDate);
  const result: any[] = []
  let currentDay = flights.length > 0 ? flights[0].scheduledDate : ''
  flights.forEach((flight) => {
    const dateKey = flight.scheduledDate;
    if (dateKey !== currentDay) {
      result.push({ isDaybreak: true, date: dateKey })
      currentDay = dateKey
    }
    result.push({ ...flight, isDaybreak: false, date: dateKey })
  })
  console.log(result);
  return result
};

function paginateFlights(flightList: any[]) {
  const pages: any[][] = []
  let currentPage: any[] = []
  let rowCount = 0

  for (const item of flightList) {
    const linesNeeded = item.isDaybreak ? 2 : 1

    if (rowCount + linesNeeded > pageSize) {
      pages.push(currentPage)
      if (pages.length >= maxPages) break
      currentPage = []
      rowCount = 0
    }

    currentPage.push(item)
    rowCount += linesNeeded
  }

  if (currentPage.length > 0) {
    pages.push(currentPage)
  }

  return pages
};


const getFullCityName = (shortCode: string): string => {
  const airport = countries.cityMap.find(a => a.codeAirport === shortCode)
      return  airport ? airport.nameAirport : 'Not Found'
};

const getRemarkClass = (flight: any) => {
    const remark = flight.remark ;
    if (remark === 'Checkin') return 'green';
    if (remark === 'Counter Closed') return 'gray';
    if (remark === 'On time') return 'blue';
    if (remark === 'Cancelled') return 'red';
    if (remark === 'Delayed') return 'yellow';
    if (remark === 'Boarding') return 'green';
    return '';
  }

const getRemark = (flight:any ) =>{
    const now = new Date();
    const start = flight.counterStart ? parse(flight.counterStart , 'dd/MM/yyyy hh:mm:ss a', new Date()) : null;
    const end = flight.counterEnd ? parse(flight.counterEnd , 'dd/MM/yyyy hh:mm:ss a', new Date()) : null;
    
    if (start && end && flight.remark != 'Cancelled') {
      //console.log("now < start: "+ now +" " + start +"  " +Boolean(now < start));
      //console.log("now >= start && now <= end: "+Boolean(now >= start && now <= end));
      //console.log("now > end: "+Boolean(now > end));
      if (now < start) return 'On time';
      if (now >= start && now <= end) return 'Checkin';
      if (now > end) return 'Counter Closed';
    }
    return flight.remark || '';
}

const formatTime = (datetime:string) :string => {
    if(datetime != "")
    {
        const date = parse(datetime, 'dd/MM/yyyy h:mm:ss a', new Date());
        if (isNaN(date.getTime())) {
            throw new Error('Invalid datetime format'+ datetime);
        }
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        return `${hours}:${minutes}`;
    }
    else
    {
        return "";
    }
};
  
const formatDateToFlightMessage = (dateString: string): string => {
    const daysOfWeek = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    const months = [
        'January', 'February', 'March', 'April', 'May', 'June',
        'July', 'August', 'September', 'October', 'November', 'December'
    ];
    const date = new Date(dateString);
    if (isNaN(date.getTime())) {
        throw new Error('Invalid date format');
    }
    const dayOfWeek = daysOfWeek[date.getDay()];
    const month = months[date.getMonth()];
    const day = date.getDate();
    const year = date.getFullYear();
    return `FLIGHTS FOR ${dayOfWeek.toUpperCase()} ${month.toUpperCase()} ${day}, ${year}`;
};


const handleImageError = (item: Flight) => {
        console.log("Error logo: "+item.lineCode);
        item.lineCode = 'trans'; // Thay đổi tên hình thay thế tại đây
      };

interface ConfigDevice 
{ id: number; name: string; location: string; description: string; ip: string;  rollOn: number;  
rollOff: number;  pageSize: number;  maxPages: number;  pageInterval: number; reloadInterval: number; 
mobilities: string; connectionId: string}

const configs = reactive<{configDevice: ConfigDevice;}>
({configDevice: {id: 1, name: "", location: "", description: "", 
ip: "", rollOn: 0, rollOff: 0, pageSize: 8, maxPages: 1, 
pageInterval:0, reloadInterval:0, mobilities: "",connectionId:""}})


const loadconfig = async () => {
  try {
    const response = await fetch(urlConfig);
    const data = await response.json();
    configs.configDevice = data;
  } catch (error) {
    console.error("Failed to load config: ", error);
  }
};

const reloadPage = async () =>{
  await loadCityMap();
  await loadconfig();
    pageSize = configs.configDevice.pageSize;
    maxPages = configs.configDevice.maxPages;
    pageInterval = configs.configDevice.pageInterval;
    reloadInterval = configs.configDevice.reloadInterval;
  await loadFlights(configs.configDevice.rollOn, configs.configDevice.rollOff);
  autoRotatePages();
}


const hubConnection = ref<signalR.HubConnection | null>(null);
const connectHub = async () => {
  const url = urlHub;
  hubConnection.value = new signalR.HubConnectionBuilder()
      .withUrl(url)
      .configureLogging(signalR.LogLevel.Information)
      .build();
    try {
      await hubConnection.value.start();
      console.log(`SignalR connection established `);
      receiverReload();
    } catch (err) {
      console.error('SignalR Connection failed to start:', err);
      startInterval(); // Khởi động lại kết nối nếu có lỗi
    }
  hubConnection.value.onclose(() => {
      console.log('Connection closed');
      startInterval();
    });
};

const intervalId = ref<number | null>(null);
const reconnectHub = async () => {
  try {
    await hubConnection.value?.start();
    console.log("SignalR reconnection established");
    receiverReload();
    stopInterval()
  } catch (err) {
    console.error("SignalR reconnection failed:", err);
  }
};

const receiverReload= () => {
{
    hubConnection.value!.on("SendToClient", (data: any) => {
        console.log("Received flight update from server:", data);

    });
}
};

const stopInterval = () => {
  if (intervalId.value !== null) {
    clearInterval(intervalId.value);
    intervalId.value = null;
  }
};

const startInterval = () => {
  if (intervalId.value !== null) {
    clearInterval(intervalId.value);
  }
  intervalId.value = window.setInterval(() => {
    reconnectHub();
  }, 30000); // 30 seconds
};

const intervalLoadFlights = ref<NodeJS.Timeout | null>(null);
onMounted(async () => {
  await reloadPage();
  intervalLoadFlights.value = setInterval(async () => 
  {
    await loadFlights(configs.configDevice.rollOn, configs.configDevice.rollOff)
  }, reloadInterval);
  
  //await connectHub()
  
});



function autoRotatePages() {
  setInterval(() => {
    currentPage.value = (currentPage.value + 1) % pagedGroupsData.value.length
  }, pageInterval)
}

const pagedGroups = computed(() => {
  const pageCount = pagedGroupsData.value.length;
  if (currentPage.value >= pageCount) {
    currentPage.value = 0; // Reset về trang đầu nếu vượt quá
  }
  //console.log("pagedGroupsData.value")
  //console.log(pagedGroupsData.value)
  //console.log(currentPage.value, pagedGroupsData.value[currentPage.value]);
  return pagedGroupsData.value[currentPage.value]
});


onUnmounted(() => {
  if (intervalId.value) {
      clearInterval(intervalId.value);
      intervalId.value = null;
  }
  if (intervalLoadFlights.value) {
    clearInterval(intervalLoadFlights.value);
    intervalLoadFlights.value = null;
  }
  });

onBeforeMount(() => {
  loadCityMap();
});


</script>
  
  <style>
  @import url("~/assets/css/icheck.css");
  body {
      text-align: center !important;
      overflow: hidden;
      background-color: #283b92;
  }
  .card-body {
      padding: 0rem !important;
  }
  
.sizerow {
    height: 6.75vh;
    color: white;
    font-size: 2vh;
}

  .sizerow td {
      padding: 10px;
      text-align: center;
      color: white;
      font-size: 3.9vh;
      vertical-align: middle; /* Căn giữa theo chiều dọc */
      /* border-bottom: 1px solid #ddd; */
  }

  .theadheader th {
      color: #121542;
      font-size: 3vh;
      padding: 10px !important;
      text-align: center;
      vertical-align: middle !important;
  }


.midal {
  vertical-align: middle !important;

}
.midal1 {
  vertical-align: middle !important;
  text-align: left !important; /* Căn chữ về bên trái */
  padding-left: 1.5vw !important;
}

  


  
  .sizerow:nth-child(odd) {
      background-color: #283b92; /* Màu nền cho hàng lẻ */
  }
  
  .sizerow:nth-child(even) {
      background-color: #121441; /* Màu nền cho hàng chẵn */
  }
  .table td, .table th {
      padding: 0.25rem;
      border-top: 0px solid #dee2e6 !important;
  }

  td img {
    text-align: center !important;
    margin: 0vh;
    width: 100%;
    display: flex;
  }
  
.notice-content {
  font-weight: bold;
  font-size: 3.9vh;
  color: white;
}
  




  td.green {
    color: #07e207;
  }
  td.red {
    color: red;
  }
  td.yellow {
    color: yellow;
  }
  
  
  </style>