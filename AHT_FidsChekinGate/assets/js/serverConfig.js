export const serverConfig = {
    urlHub: 'http://172.17.18.12:8083/dashboardHub',
    ulrCheckDevice: 'http://172.17.18.12:8085/api/FidsLocation',
    urlDep: 'http://172.17.18.12:8085/api/FidsSpare',
    urlCountries: 'http://172.17.18.12:8085/api/Countries',
    urlArr: 'http://172.17.18.12:8085/api/Arrivals',
    urlFlight: `http://172.17.18.12:8085/api/CheckinCounter`,
    urlGate: `http://172.17.18.12:8085/api/Gate`,
    urlCheckinCounter: 'http://172.17.18.12:8085/api/CheckinCounter/',
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