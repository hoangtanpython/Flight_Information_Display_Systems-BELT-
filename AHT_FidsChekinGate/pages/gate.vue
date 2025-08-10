<template>
  <div v-if="currentFlight">
    <div class="header">
        <div class="logo">
            <img class="logoaht" :src="`/img/logo/AHTLogo2.png?timestamp=${new Date().getTime()}`" alt="" title="">
        </div>
        <div class="text">
            <!-- <img class="logoaht" src="/img/logo/Icon-Arrival.png" alt="" title=""> -->
            <p >GATE {{ currentFlight.gate }}</p>
        </div>
    </div>
    <div class="mainboard form-group row styleFromcontrol">
      <div class="col-sm-6">
        <div class="row" style="padding-left: 5vw;">
          <div class="timemcdt">
            <span>FLIGHT</span>
            <span class="mcdt">{{ currentFlight.flight }}</span>
          </div>
        </div>
        <div class="row" style="padding-left: 5vw;">
          <div class="timemcdt">
            <span>CITY TO</span>
            <span  class="mcdt">{{ getFullCityName(currentFlight.city) }}</span>
          </div>
        </div>
        <div class="row" style="padding-left: 5vw;">
          <div class="timemcdt">
            <span>STATUS</span>
            <span  class="mcdt">{{ currentFlight.remark }}</span>
          </div>
        </div>
        <div class="row" style="padding-left: 5vw;">
          <div class="timemcdt">
            <span>NOTE</span>
            <span  class="mcdt"></span>
          </div>
        </div>
      </div>
      <div class="col-sm-6">
        <div class="row">
          <div class="col-sm-6 timemcdt">
            <span>TIME</span>
            <span class="mcdt">{{ getTimeMcdt(currentFlight.mcdt) }}</span>
          </div>
          <div class="col-sm-6 timemcdt">
            <span></span>
            <span  class="mcdt">{{ gettemprate(currentFlight.city) }}</span>
          </div>
        </div>
        <div class="logoairline">
            <div >
                <img alt="Vuelogo" :src="`/img/fullscreen/${currentFlight.lineCode}_1920x1080.png?timestamp=${new Date().getTime()}`" @error="handleImageError(currentFlight)" style="overflow: hidden;"/>
            </div>
        </div>
      </div>
    </div>
    <div class="footer">
        <p class="date">{{ date  }}</p>
    </div>
  </div>
  <div v-else>
      <img  alt="Vuelogo" :src="`/img/fullscreen/AHT_1920x1080.png?timestamp=${new Date().getTime()}`" />
  </div>
    
</template>
  
  <script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { format , parse } from 'date-fns'
  import * as signalR from "@microsoft/signalr";

  //  const urlHub = 'https://localhost:7248/dashboardHub';
  // const urlGate = 'https://localhost:7079/api/Gate';
  // const urlCountries = 'https://localhost:7079/api/Countries';
  
  const urlHub = 'http://172.17.18.12:8084/dashboardHub';
  const urlGate = 'http://172.17.18.12:8085/api/Gate';
  const urlCountries = 'http://172.17.18.12:8085/api/Countries';
  const timeCheckFlight = 30;
  const timeLoadFlight = 40;


  definePageMeta({
  layout: 'checkinlayout' // Chỉ định layout custom
  })
  
  interface Flight {
    id: string;scheduledDate: string;schedule: string;estimated: string;actual: string;lineCode: string;
    flight: string;city: string;gate: string;remark: string;status: string;rowFrom: string;rowTo: string;
    checkInCounters: string;counterStart: string;counterEnd: string;gateStart: string;gateEnd: string;mcdt: string;
  }
  
  const flights = ref<Flight[]>([]);
  const currentFlight = ref<Flight | null>(null);
  const date = ref("");
  const countries = reactive({
      cityMap: [
          {codeAirport: "BHY",nameAirport: "Beihai",countries: "Trung Quốc"},
          {codeAirport: "ICN",nameAirport: "Incheon",countries: "Hàn Quốc"}]
  });

  const fetchFlights = async () => {
    const headers = new Headers();
    headers.append('Cache-Control', 'no-cache, no-store, must-revalidate');
    headers.append('Pragma', 'no-cache');
    headers.append('Expires', '0');
    fetch(urlGate) 
    .then(response => response.json())
    .then(data => {
      flights.value = data;
      checkCurrentFlight(); // Kiểm tra chuyến bay hiện tại
    })
    .catch(error => {
      console.error(error);
    });
  };

  // Hàm để chuyển đổi chuỗi "mcdt" thành đối tượng Date hợp lệ
    const parseDate = (dateString: string): Date => {
    // Chuẩn hóa chuỗi ngày, loại bỏ khoảng trắng thừa và thêm khoảng trắng trước AM/PM
    const normalizedDateString = dateString.replace(/\s+/g, ' ').replace(/(\d+)(AM|PM)$/, '$1 $2');
    return new Date(normalizedDateString);
    };
  
  const checkCurrentFlight = () => {
    const now = new Date();
    const filteredFlights = flights.value.filter((flight) => {
    const mcdtDate = parseDate(flight.mcdt);
    const timeBefore = new Date(mcdtDate.getTime() - 150 * 60 * 1000); // 150 phút trước
    const timeAfter = new Date(mcdtDate.getTime() + 20 * 60 * 1000); // 20 phút sau
    return now >= timeBefore && now <= timeAfter;});
    if (filteredFlights.length > 0) {
        filteredFlights.sort((a, b) => new Date(a.mcdt).getTime() - new Date(b.mcdt).getTime());
        currentFlight.value = filteredFlights[0];
        console.log(currentFlight.value);
    } else {
        currentFlight.value = null;
    }
    
  };

  const getdatefooter =() => {
    date.value =  format(new Date(), 'dd MMMM yyyy |HH:mm:ss')
  };

  const getTimeMcdt = (mcdt: string): string => {
    const date = parseDate(mcdt);
    const hours = date.getHours(); // Lấy giờ theo định dạng 24h
    const minutes = date.getMinutes(); // Lấy phút
    const formattedHours = hours.toString().padStart(2, '0');
    const formattedMinutes = minutes.toString().padStart(2, '0');
    return `${formattedHours}:${formattedMinutes}`;
  };

  const gettemprate = (city: string): string => {
    return ""
  }

  const handleImageError = (item: Flight) => {
    item.lineCode = 'AHT'; // Thay đổi tên hình thay thế tại đây
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
        return  airport ? airport.nameAirport : 'Not Found'
  };

  const intervalId = ref<number | null>(null);
  const startInterval = () => {
  if (intervalId.value !== null) {
    clearInterval(intervalId.value);
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
        //receiverUpdate();
      } catch (err) {
        console.error('SignalR Connection failed to start:', err);
        startInterval(); // Khởi động lại kết nối nếu có lỗi
      }
    hubConnection.value.onclose(() => {
        console.log('Connection closed');
        startInterval();
      });
  };

  const reconnectHub = async () => {
    try {
      await hubConnection.value?.start();
      console.log("SignalR reconnection established");
      //receiverUpdate();
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
    await fetchFlights(); // Gọi API cập nhật dữ liệu flights trước
    loadCityMap();
    getdatefooter;
    setInterval(getdatefooter, 1000);
    setInterval(checkCurrentFlight, timeCheckFlight*1000); // Cập nhật chuyến bay mỗi phút
    setInterval(fetchFlights, timeLoadFlight*1000);  // Gọi API cập nhật flights mỗi 5 phút (300000ms)
  });
}
</script>
  
  <style scoped>
.footer {
    background-color: #36c0c7;
    left: 0px !important;
    width: 100vw;
    height: 6.6vh;
    position: fixed;
    bottom: 0px;
    padding: 5px;
}
p.date {
    color: #2b388f;
    padding-right: 4vw;
    font-size: 3.5vh;
    float: right;
    padding-top: 1.8vh;
    font-weight: 600;
}
/* ///////// */
.mainboard {
    /* Đặt chiều rộng và chiều cao cho mainboard, bạn có thể điều chỉnh tùy theo yêu cầu */
    width: 100%; /* Hoặc kích thước cụ thể như 100vw, 1200px, v.v. */
    /*height: 100%;  Hoặc chiều cao cụ thể như 100vh, 800px, v.v. */
    display: flex; /* Giúp căn giữa nội dung */
    /*justify-content: center; /* Căn giữa theo chiều ngang */
    /*align-items: center; /* Căn giữa theo chiều dọc */
    overflow: hidden; /* Giấu phần nội dung thừa */
    position: relative; /* Để xử lý bố cục bên trong */
    height: 73.3vh;
    background-color: #283b92;
    margin-right: 0px !important;
    margin-left: 0px !important;
}

.logoairline {
    width: 99%;
    position: relative;
    overflow: hidden;
    display: flex;
    justify-content: center;
    align-items: center;
    background-color: #fff;
    border-radius: 3px;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    margin-top: 4vh;
    /* left: 23.7vw; */
    /* top: 10vh; */
}

.logoairline img {
    width: 100%; /* Đảm bảo hình ảnh co dãn theo chiều rộng của khung */
    height: 100%; /* Đảm bảo hình ảnh co dãn theo chiều cao của khung */
    object-fit: cover; /* Giúp hình ảnh che phủ toàn bộ khung mà không bị méo */
    display: block; /* Loại bỏ khoảng cách dưới ảnh */
}

.timemcdt {
    display: flex;
    flex-direction: column;
    color: #36c0c7;
    font-size: 3.8vh;
    align-items: flex-start;
    margin-top: 5vh;
    font-weight: 700;
}

span.mcdt {
  font-size: 11vh;
    padding-top: 5vh;
    padding-bottom: 4vh;
    color: white;
    font-weight: 700;
}
/* //////// */

/* .mainboard {
    height: 73.3vh;
    background-color: #4057f0;
} */


.header {
    background-color: #ffffff;
    display: flex;
    flex-direction: row;
    align-items: center;
    padding: 0px 0.5rem;
    height: 20vh;
}
.logoaht {
    max-height: 100%;
    width: auto;
}
.logo {
    height: 100%;
    /* width: 40vw; */
}
.text {
    color: #121441;
    font-weight: 700;
    font-size: large;
    margin-left: 22px;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 60vw;
    height: 100%;
}
p {
    /* margin-top: 0; */
    margin-bottom: 0.5rem;
    display: flex;
    justify-content: center;
    /* height: 100%; */
    color: #2b388f;
    font-weight: 700;
    font-size: 5.2vw;
    /* padding-top: 7%; */
}
  </style>
  