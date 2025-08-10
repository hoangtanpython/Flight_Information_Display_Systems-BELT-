export const localConfig = {
    urlHub: 'https://localhost:7248/dashboardHub',
    ulrCheckDevice: 'https://localhost:7079/api/FidsLocation',
    urlDep: 'https://localhost:7079/api/FidsSpare',
    urlCountries: 'https://localhost:7079/api/Countries',
    urlArr: 'https://localhost:7079/api/Arrivals',
    urlFlight: 'https://localhost:7079/api/CheckinCounter',
    urlGate: 'https://localhost:7079/api/Gate',
    urlCheckinCounter: 'https://localhost:7079/api/CheckinCounter/',
    numberfload: 3, //số trang lật template DepLed
    timefresh: 65,  //Thời gian làm mới dữ liệu
    timeflop: 25, //Thời gian lật trang tính bằng s
    numberfloadArr: 2, //số trang lật của template Arr
    timeflopArr: 25, //Thời gian lật trang tính bằng s
    numberfloadDep: 2, //số trang lật template Dep Gate
    timeflopDep: 25, //Thời gian lật trang tính bằng s dep gate
    timeCheckFlight: 50, //thời gian check flight tính bằng s template Chekin
    timeLoadFlight: 60 //Thời gian làm mới data template Checkin

};