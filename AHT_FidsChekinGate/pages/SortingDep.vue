<template>
    <div class="headerAd">
        <div class="logo">
            <img class="logoaht" src="/img/logo/LogoBgW.png" alt="" title="">
        </div>
        <div class="textAd" style="color: #2b388f !important;">
            <p>{{ configs.configDevice.name }}</p>
        </div>
    </div>
    <div class="table-responsive" style="width: 100vw">
        <table class="" style="height: 100%;">
            <thead class="theadheader" style="background-color: #36c0c7; height: 6.5vh;">
                <tr>
                    <th style="width: 10vw;">STD</th>
                    <th style="width: 10vw;">ETD</th>
                    <th style="width: 15vw;">FLIGHT</th>
                    <th style="width: 23vw; ">AIRLINE</th>
                    <th style="width: 27vw; "><spam style="float: left; padding-left: 2.4vw;">DESTINATION</spam></th>
                    <th style="width: 15vw;">STATUS</th>
                </tr>
            </thead>
            <tbody style="height: 67vh;">
                <tr class="sizerow" v-for="item in flights" :key="item.id">
                        <td>{{ formatTime(item.schedule) }}</td>
                        <td>{{ formatTime(item.estimated) }}</td>
                        <td>{{ item.flight }}</td>
                        <td><img  alt="Vuelogo" :src="`/img/logo/${item.lineCode}_240x38.png`" @error="handleImageError(item)" /></td>
                        <td style="text-align: start; padding-left: 2.4vw;">{{ getFullCityName(item.city) }}</td>
                        <td class="logoStatus"><img :class="{ blink: shouldBlink(item) }"  :src="`${getStatusIcon(item)}?timestamp=${new Date().getTime()}`" v-if="getStatusIcon(item)" alt="" title=""></td>
                    </tr>
            </tbody>
        </table>
    </div>
    <HomeAppFooter /> 
</template>

<script setup lang="ts">
 import { ref, computed, onMounted } from 'vue';
 import * as signalR from "@microsoft/signalr";
 const isLoading = ref(false)
 import { parse } from 'date-fns';
 const timefresh = 95;
 interface FlightInfo {
    lineCode: string;
    flight: string;
    city: string;
    mcdt: string;
    schedule: string;  
    estimated: string;
    counterStart: string;
    counterEnd: string;
    rowFrom: string;
    rowTo: string;
    checkInCounters: string;
    remark: string;
    status: string;
    id: string;
};
const flights = ref<FlightInfo[]>([])

const countries = reactive({
    cityMap: [
        {codeAirport: "BHY",nameAirport: "Beihai",countries: "Trung Quốc"},
    ]
}
);



const urlCountries = 'http://172.17.18.12:8085/api/Countries';
const urlHub = 'http://172.17.18.12:8084/dashboardHub';
const urlConfig = 'http://172.17.18.12:8085/api/Sorting';
const urlbase = 'http://172.17.18.12:8085';


// const urlHub = 'https://localhost:7248/dashboardHub';
// const urlCountries = 'https://localhost:7079/api/Countries';
// const urlConfig = 'https://localhost:7079/api/Sorting';
// const urlbase = 'https://localhost:7079/';

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
    if (intervalId.value !== null) {
      clearInterval(intervalId.value);
      intervalId.value = null;
    }
  } catch (err) {
    console.error("SignalR reconnection failed:", err);
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

const getFullCityName = (shortCode: string): string => {
const airport = countries.cityMap.find(a => a.codeAirport === shortCode)
    return  airport ? airport.nameAirport : ''
};

  
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

const handleImageError = (item: any) => {
        item.lineCode = 'trans';
    };

const getStatusIcon = (item: any): string => {
  const now = new Date();

  const start = item.counterStart ? new Date(item.counterStart) : null;
  const end = item.counterEnd ? new Date(item.counterEnd) : null;
  //const status = item.remark?.toUpperCase() || ""; // e.g., 'ON', 'DP'
  const status = item.remark?.toUpperCase() || ""; // e.g., 'ON', 'DP'
  if(end !== null && start !== null)
  {
    console.log(item.flight + " - "+"Remark: " + item.remark+ " - now > end: "+ Boolean(now > end) +" - "+"now < start: "+ Boolean(now < start));

    // Nếu thiếu dữ liệu thời gian → không hiển thị gì
    if (!start || !end || isNaN(start.getTime()) || isNaN(end.getTime())) {
      return "";
    }

    // Nếu current time < counterStart → chưa đến giờ check-in
    if (new Date(now) < new Date(start)) {
      return "";
    }

    // Nếu trong khoảng counterStart <= now <= counterEnd
    if (new Date(now) >= new Date(start) && new Date(now) <= new Date(end)) {
      return "/img/icon/Yellow-dot.png";
    }

    // Nếu current > counterEnd và status là ON, OT, BD hoặc rỗng
    const statusOk = ["On time", "Boarding",  ""].includes(item.remark);
    if (now > end && statusOk) {
      return "/img/icon/Green-dot.png"; // Có thể dùng thêm blink ở frontend
    }

    // Nếu current > counterEnd và status là DP, FC
    if ((now > end) && (["Departed", "Final call" ,"Gate closed" ,"Last call" ].includes(item.remark))) {
      return "/img/icon/Red-dot.png";
    }

    // Mặc định: không hiển thị icon
    return "";
  } 
  else
  {
    return "";
  }
};

const shouldBlink = (item: any) => {
  const now = new Date();
  const end = new Date(item.counterEnd);
  const status = item.remark?.toUpperCase() || "";
  return now > end && ["On time", "Delayed", "Boarding","Last call", ""].includes(item.remark);
}

const useFlightSorting = async (top: number,  rollOn: number,  rollOff: number,  location: string,  numberM: number): Promise<FlightInfo[]> => 
{
  console.log(top , rollOn, rollOff, location, numberM);
  const config = useRuntimeConfig()
  const { data, error } = await useFetch<FlightInfo[]>('/api/SortingDep', {
    baseURL: urlbase,
    method: 'GET',
    query: { top, rollOn, rollOff, location, numberM }
  });
   flights.value = data.value ?? [];
  if (error.value) {
    console.error('❌ API Error:', error.value)
    return []
  }

  return flights.value ?? []
}

interface ConfigDevice { id: number; name: string; location: string; description: string; ip: string;  rollOn: number;  
                         rollOff: number;  pageSize: number;    connectionId: string;}

const configs = reactive<{configDevice: ConfigDevice;}>
({configDevice: {id: 1, name: "", location: "CKA", description: "", ip: "", rollOn: 0, rollOff: 24, pageSize: 8,  connectionId:""}})
const loadconfig = async () => {
  try {
    const response = await fetch(urlConfig);
    const data = await response.json();
    configs.configDevice = data;
    console.log(configs.configDevice.pageSize);
  } catch (error) {
    console.error("Failed to load config: ", error);
  }
};

const intervalLoadShort = ref<NodeJS.Timeout | null>(null);
onMounted(async () => {
    await loadCityMap();
    await loadconfig();
    
    await useFlightSorting(configs.configDevice.pageSize, configs.configDevice.rollOn, configs.configDevice.rollOff, configs.configDevice.location, 3)

    intervalLoadShort.value = setInterval(async () => 
    {
      await useFlightSorting(configs.configDevice.pageSize, configs.configDevice.rollOn, configs.configDevice.rollOff, configs.configDevice.location, 3);
    }, timefresh * 1000);
    //await connectHub();
});

onUnmounted(async () => {
  if (intervalId.value) {
    clearInterval(intervalId.value);
  }
  if (hubConnection.value) {
    hubConnection.value.stop().then(() => {
        console.log('SignalR connection stopped');
    });
  }
  if (intervalLoadShort.value) {
    clearInterval(intervalLoadShort.value);
    intervalLoadShort.value = null;
  }

});

</script>

<style>
body {
    background-color: #283b92;
}
  .sizerow {
    height: 6.5vh;
    text-align: center;
    color: white;
    font-size: 4vh;
  } 
.textAd p {
    color: #2b388f !important;
}

.headerAd {
    background-color: #ffffff ;
    display: flex;
    flex-direction: row;
    align-items: center;
    padding: 2px 0.5rem;
    height: 20vh;
}
.logoaht {
    max-height: 90%;
    width: auto;
    vertical-align: middle;
    padding-top: 1%;
}
.logo {
    height: 100%;
    width: 55vw;
}
.textAd {
    color: #2b388f !important;
    font-weight: 700;
    font-size: large;
    margin-left: 22px;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 45vw;
    height: 100%;
}
p {
    color: #2b388f;
    font-weight: 700;
    font-size: 8vw;
    align-items: center;
}
thead.theadheader {
    height: 100%;
    color: #121542;
    font-size: 4vh;
    text-align: center;
}

  .sizerow:nth-child(odd) {
      background-color: #121441; /* Màu nền cho hàng lẻ */
  }
  
  .sizerow:nth-child(even) {
      background-color: #283b92; /* Màu nền cho hàng chẵn */
  }

  td img {
    text-align: center !important;
    margin: 0vh;
    width: 100%;
  }

  .logoStatus img {
    height: 6.2vh;
    width: auto;
}

.blink {
  animation: blink-animation 1.5s steps(2, start) infinite;
}

@keyframes blink-animation {
  to {
    visibility: hidden;
  }
}
</style>