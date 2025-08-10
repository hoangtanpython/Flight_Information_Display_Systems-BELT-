<template>
    <div>
        <div v-if="Model">
          <div v-if="Nomal" style="">
            <img alt="Vuelogo" :src="`/img/1920x480/${showImghafl}?timestamp=${new Date().getTime()}`" @error="handleImageError2(showImghafl)" 
            style="overflow: hidden;max-width: 100vw;height: 44.5vh; background-color: #244093;"/>
            <div class="noidungtext" style="background-color: #244093; max-width: 100vw;height: 55.5vh;">
               <span>{{ destination }}</span>
               <div class="flightTime">
                <span>{{ flight }}</span>
                <span>{{ time }}</span>
               </div>
            </div>
          </div>
          <div v-else>
            <img alt="Vuelogo" :src="`/img/fullscreen/${showImg}?timestamp=${new Date().getTime()}`" @error="handleImageError(showImg)" 
            style="overflow: hidden;max-width: 100vw;height: auto;"/>
          </div>
        </div>
        <div v-else>
          <img 
            :alt="'Vuelogo'"
            :src="`/img/fullscreen/${images[currentIndex]}?timestamp=${new Date().getTime()}`"
            class="slideshow-image"
          />
        </div>
    </div>
</template>
  <script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import * as signalR from "@microsoft/signalr";
  const Model = ref(false);
  const Nomal = ref(true);
  const showImgPre = ref("");
  const showImg = ref("");
  const showImghafl = ref("");
  const destination = ref("");
  const flight = ref("");
  const time = ref("");
  const nameCounter = ref<string | null>(null);
  let timeClose = ref<Date | null>(null);
  let timeStart = ref<Date | null>(null);

 
  const urlCountries = 'https://localhost:7248/api/Countries';
  const urlHub = 'https://localhost:7248/hub/flight';

  //  const urlHub = 'https://localhost:7248/dashboardHub';
  //  const urlCountries = 'https://localhost:7079/api/Countries';
   

// Danh sách ảnh (cập nhật nếu có ảnh mới)
const images = ref<string[]>([
  "AHT1.png",
  "AHT2.png",
  "AHT3.png",
  "AHT4.png"
]);

const countries = reactive({
  cityMap: [
      {codeAirport: "BHY",nameAirport: "Beihai",countries: "Trung Quốc"},
      {codeAirport: "ICN",nameAirport: "Incheon",countries: "Hàn Quốc"}
  ]});

const currentIndex = ref<number>(0);
let intervalIdaht: NodeJS.Timeout | null = null;
const changeImage = () => {
  currentIndex.value = (currentIndex.value + 1) % images.value.length;
};


const hubConnection = ref<signalR.HubConnection | null>(null);
const connectHub = async () => {
  const url = urlHub;
  hubConnection.value = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7248/hub/flight')
      .configureLogging(signalR.LogLevel.Information)
      .build();
    try {
      await hubConnection.value.start();
      console.log(`SignalR connection established `+ nameCounter.value);
      receiverUpdate();
    } catch (err) {
      console.error('SignalR Connection failed to start:', err);
      startInterval(); // Khởi động lại kết nối nếu có lỗi
    }
  hubConnection.value.onclose(() => {
      console.log('Connection closed');
      startInterval();
    });
};

const receiverUpdate= () => {
{
    // Lắng nghe sự kiện "SendToClient" từ server
    hubConnection.value!.on("SendToClient", (data: any) => {
        console.log("Received flight update from server:", data);
        //console.log(`Received flight update from server to `+ nameCounter.value);

        timeStart.value = new Date(`${data.openTime}`);
        timeClose.value = new Date(`${data.closeTime}`);
        destination.value = getFullCityName(`${data.setImg}`);
        flight.value = `${data.flight}`;
        time.value = formattedTime(`${data.timeMcdt}`);
        nameCounter.value = `${data.name}`;
        Model.value = data.auto === "False" || new Date() < timeStart.value ? false : true;
        Nomal.value = data.mode === "Nomal" &&  new Date() < timeClose.value &&  new Date() > timeStart.value ? true : false;
        // showImgPre.value = !`${data.setImg}` || `${data.setImg}` === "null" ? !`${data.autoImg}` || `${data.autoImg}` === "null" ? `AHT_1920x1080.png` : `${data.autoImg}_1920x1080.png` : `${data.setImg}`;
        //showImg.value = `${data.mode}` === "Eco" ? `${data.eco} ? ${data.autoImg}_1920x1080.png : ${data.eco}` : `${data.bus} ? ${data.autoImg}_1920x1080.png : ${data.bus} `;
        showImg.value = data.mode === "Eco"
        ? (!data.eco ? `${data.autoImg}_1920x1080.png` : `${data.eco}`)
        : (!data.bus ? `${data.autoImg}_1920x1080.png` : `${data.bus}`);
        //showImg.value = new Date() < timeStart.value ? `AHT_1920x1080.png` : showImgPre.value;
        showImghafl.value = !`${data.nomal}` || `${data.nomal}` === null ? `${data.autoImg}_1920x480.png` : `${data.nomal}`;
        console.log(`${data.nomal}` + ' ' + showImghafl.value + ' ' +showImg.value);
        startCheckingFlights();
        console.log(`Name: ${data.name}`);
    });
}
};

const formattedTime = (bien: string) => {
  const date = new Date(bien);
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${hours}:${minutes}`;
};

const loadCityMap = async () => {
  try {
    const response = await fetch(urlCountries);
    const data = await response.json();
    countries.cityMap = data;
  } catch (error) {
    console.error('Không thể load countries, dùng dữ liệu mặc định.');
    // fallback nếu fetch lỗi
    countries.cityMap = [
      { codeAirport: "BHY", nameAirport: "Beihai", countries: "Trung Quốc" },
      { codeAirport: "ICN", nameAirport: "Incheon", countries: "Hàn Quốc" }
    ];
  }
};

  
const getFullCityName = (shortCode: string): string => {
  const airport = countries.cityMap.find(a => a.codeAirport === shortCode)
      return  airport ? airport.nameAirport : 'Not Found'
};

const handleImageError = (item: string) => {
  console.log("Error item");
  showImg.value = 'AHT_1920x1080.png';
};

const handleImageError2 = (item: string) => {
   console.log(item);
   showImghafl.value = 'Logo_1920x480.png';
};

// Kiểm tra thời gian hiện tại > CloseTime
const checkFlightsAndDelete = () => {
  if (!timeClose.value || !timeStart.value) {
    console.log("No CloseTime set. Skipping check.");
    return;
  }
  const currentTime = new Date();
  const closeTime = timeClose.value;
  const startTime = timeStart.value;
  if (currentTime > closeTime) {
    console.log("Checkin counter is closed. Perform necessary cleanup.");
    if (hubConnection.value) {
      hubConnection.value
        .invoke("DeleteFlightFromClient", nameCounter.value) 
        .then((result) => { // Nhận kết quả từ server
          if (result) {
            console.log("Flight deleted successfully.");
            stopCheckingFlights();
          } else {console.log("Failed to delete flight.");}
        })
        .catch((err) => {
          console.error("Error sending delete request:", err);
        });
    } else {console.error("SignalR connection is not established.");}
  }else
  {
    if(currentTime < startTime)
    {
      Model.value = false;
    }else
    {Model.value =true;}
  }
};
const intervalId1 = ref<number | null>(null);
const startCheckingFlights = () => {
  if (intervalId1.value !== null) {
    clearInterval(intervalId1.value);
  }
  intervalId1.value = window.setInterval(() => {
    checkFlightsAndDelete();
  }, 30000); // 60000ms = 1 phút
};
// Hàm dừng kiểm tra
const stopCheckingFlights = () => {
  if (intervalId1.value !== null) {
    clearInterval(intervalId1.value);
    intervalId1.value = null;
  }
};

const intervalId = ref<number | null>(null);
const reconnectHub = async () => {
  try {
    await hubConnection.value?.start();
    console.log("SignalR reconnection established");
    receiverUpdate();
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


onMounted(async () => {
  if (intervalId.value !== null) {
      clearInterval(intervalId.value);
      intervalId.value = null;
  }
  await connectHub();
  loadCityMap();
  intervalIdaht = setInterval(changeImage, 15000);
});

onUnmounted(() => {
    stopCheckingFlights();
    if (intervalId.value) clearInterval(intervalId.value);
    if (hubConnection.value) {
    hubConnection.value.stop().then(() => {
        console.log('SignalR connection stopped');
    });
    }

    if (intervalIdaht) clearInterval(intervalIdaht);

});
</script>
  
<style scoped>

.noidungtext {
    font-size: 15vh;
    font-weight: bold;
    color: white;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: space-evenly;
}

.flightTime {
    display: flex;
    flex-direction: row;
    justify-content: space-around;
    width: 100vw;
}

</style>
  