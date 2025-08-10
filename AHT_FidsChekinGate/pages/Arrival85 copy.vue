<template>
    <HomeAppHeaderArr85 />
    <section class="content">
      <div class="row">
              <div class="card-body pad table-responsive" style="width: 100vw">
                      <table class="table table-hover" id="tblWorkOrderDone">
                          <thead class="classtheadheader" style="background-color: #36c0c7;height: 4vh;">
                              <tr>
                                  <th style="width: 13vw;">TIME</th>
                                  <th style="width: 19vw;">AIRLINE</th>
                                  <th style="width: 14vw;">FLIGHT</th>
                                  <th style="width: 25vw; "><spam >FROM</spam></th>
                                  <th style="width: 13vw;">ARRIVAL</th>
                                  <th style="width: 16vw;">REMARKS</th>
                              </tr>
                          </thead>
                          <tbody>
                              <template v-for="item in displayedFlights" :key="item.id">
                                  <tr v-if="item.schedule == ''" class="sizerow notice-row" style="height: 11vh;">
                                      <td colspan="6" style="vertical-align: middle;">
                                      <div class="notice-content">{{ formatDateToFlightMessage(item.scheduledDate) }}</div>
                                      </td>
                                  </tr>
                                  <tr v-else class="sizerow85">
                                      <td class="midal">{{ formatTime(item.schedule) }}</td>
                                      <td class="midal"><img  alt="Vuelogo" :src="`/img/logo/${item.lineCode}_240x38.png?timestamp=${new Date().getTime()}`" @error="handleImageError(item)" /></td>
                                      <td class="midal">{{ item.flight }}</td>
                                      <td class="midal1" style="" >{{ getFullCityName(item.city) }}</td>
                                      <td class="midal">{{ ((item.estimated == "")&&(item.actual == "")) ? formatTime(item.schedule) : (item.actual == "" ? (item.estimated != "" ? formatTime(item.estimated): "" )  : formatTime(item.actual)) }}</td>
                                      <td class="midal" :class="item.remark == 'Arrived'? 'green':item.remark == 'Cancelled'? 'red':item.remark == 'Delayed'? 'yellow':''">{{ item.remark }}</td>
                                  </tr>
                                  
  
                              </template>
  
                          </tbody>
                      </table>
                  </div>
          </div >
    </section>
    <HomeAppFooter85 /> 
  </template>
  
  <script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { parse, format } from 'date-fns'
  import { watch, reactive, toRefs } from 'vue';
  const currentIndex = ref(0);
  const rowsPerPage = 15;
  const specialIndex = ref(0);

  const urlCountries = 'https://localhost:7079/api/Countries';
  const urlArr = 'https://localhost:7079/api/Arrivals';

  // const urlCountries = 'http://172.17.18.12:8085/api/Countries';
  // const urlArr = 'http://172.17.18.12:8085/api/Arrivals';
  const numberfloadArr = 1;
  const timeflopArr = 15;
  const timefresh = 151;


  let foundSpecialFlight = false;
  let timestart = "2024-09-16";
  const intervalId = ref<NodeJS.Timeout | null>(null);
  const intervalIdFres = ref<NodeJS.Timeout | null>(null);
  const countries = reactive({
    cityMap: [
        {codeAirport: "BHY",nameAirport: "Beihai",countries: "Trung Quốc"},
        {codeAirport: "ICN",nameAirport: "Incheon",countries: "Hàn Quốc"}
    ]});

  interface Flight {
    id: string;scheduledDate: string;schedule: string;estimated: string;
    actual: string;lineCode: string;flight: string;city: string;belt: string;remark: string;
  };
  const flights = ref<Flight[]>([]);
  
  const displayedFlights = computed(() => {
    const data = flights.value;
    const result: Flight[] = [];
    let lastDate = "";
    let j = 0;
    if(data.length >0)
    {
      for (let i = currentIndex.value; i < data.length; i++) {
      const item = data[i];
      j = j +1;   
      if (lastDate && lastDate !== item.scheduledDate) {
        j = j +1; 
        if (j == rowsPerPage-1) {
          result.push({ id: item.id, scheduledDate: item.scheduledDate, schedule: "", estimated: item.estimated, 
                        actual: item.actual, lineCode:item.lineCode, flight: item.flight, 
                        city:item.city, belt:item.belt, remark:item.remark });
                        specialIndex.value = 1;
          break;
        }
        if(j>=rowsPerPage){
          break;
        }
        result.push({ id: item.id, scheduledDate: item.scheduledDate, schedule: "", estimated: item.estimated, 
              actual: item.actual, lineCode:item.lineCode, flight: item.flight, 
              city:item.city, belt:item.belt, remark:item.remark });
              j = j +1;
              specialIndex.value = 2;
      }
      result.push(item);
      lastDate = item.scheduledDate;
      if (j == rowsPerPage) {
        break;
      }
      }
    }
    return result;
    });

    const updateFlights = () => {
    // Chuyển đến phần tiếp theo của danh sách
    currentIndex.value += rowsPerPage;
    if ((currentIndex.value >= flights.value.length)||(currentIndex.value > numberfloadArr*15 -2)) {
      currentIndex.value = 0;
    }
    if (specialIndex.value == 2)
    {
      currentIndex.value = currentIndex.value >0 ?  currentIndex.value-2 : 0;
      specialIndex.value = 0;
    }
    if (specialIndex.value == 1)
    {
      currentIndex.value = currentIndex.value >0 ? currentIndex.value -1 :0;
      specialIndex.value = 0;
    }
    if (flights.value.length - currentIndex.value < 6)
    {
      currentIndex.value = 0;
      specialIndex.value = 0;
    }
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
  const refetchDataArr = () => {
  fetch(urlArr) 
    .then(response => response.json())
    .then(data => {
      if (Array.isArray(data)) { // Kiểm tra xem có phải là mảng không
        currentIndex.value = 0;
        flights.value = data;
        console.log("refetchDataArr ok!");
      } else {
        console.error("Invalid data format from API");
      }
    })
    .catch(error => {
      console.error("Error fetching data:", error);
    });
  };
  
  const getFullCityName = (shortCode: string): string => {
    const airport = countries.cityMap.find(a => a.codeAirport === shortCode)
        return  airport ? airport.nameAirport : 'Not Found'
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
         console.log(item.lineCode);
         item.lineCode = 'trans'; // Thay đổi tên hình thay thế tại đây
         console.log(item.lineCode);
        };

  function startInterval() {
    intervalId.value = setInterval(updateFlights, timeflopArr*1000);
  };

  function stopInterval() {
    if (intervalId.value) {
        clearInterval(intervalId.value);
        intervalId.value = null;
    }
  };

  function stopIntervalFres() {
    if (intervalIdFres.value) {
        clearInterval(intervalIdFres.value);
        intervalIdFres.value = null;
    }
  };
  const intervalIdFresArr = ref<NodeJS.Timeout | null>(null);
  onMounted(() => {
    startInterval();
    refetchDataArr();
    intervalIdFresArr.value = setInterval(refetchDataArr, timefresh*1000);
  });
  onUnmounted(() => {
    if (intervalId.value) {
        clearInterval(intervalId.value);
        intervalId.value = null;
    }
    if (intervalIdFresArr.value) {
        clearInterval(intervalIdFresArr.value);
        intervalIdFresArr.value = null;
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
  
.sizerow85 {
    height: 5.5vh;
    color: white;
    font-size: 2vh;
}
.midal {
  vertical-align: middle !important;

}
.midal1 {
  vertical-align: middle !important;
  text-align: left !important; /* Căn chữ về bên trái */
  padding-left: 1.5vw !important;
}

  
  .classtheadheader th {
      height: 4.5vh;
      color: #121542;
      font-size: 1.7vh;
      padding: 10px !important;
      text-align: center;
      vertical-align: middle !important;
  }

  
  .sizerow85:nth-child(odd) {
      background-color: #121441; /* Màu nền cho hàng lẻ */
  }
  
  .sizerow85:nth-child(even) {
      background-color: #283b92; /* Màu nền cho hàng chẵn */
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
  font-size: 3vh;
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