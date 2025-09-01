export default defineNuxtConfig({
  compatibilityDate: '2024-04-03',
  ssr: false,
  devtools: { enabled: true },
  css: [
    "~/assets/css/bootstrap/bootstrap.min.css",
    "~/assets/css/fontawesome/fontawesome-all.min.css",
    "~/assets/css/fontawesome/all.min.css",
    "~/assets/css/flaticon/flaticon.css",
    "~/assets/css/default.css",
    "~/assets/css/style.css",
    "~/assets/css/responsive.css"
  ],
  runtimeConfig: {
    public: {
      
      apiBase: 'https://localhost:7248/api',     
      hubBase: 'https://localhost:7248',         

      urlCheckDevice: 'https://localhost:7248/api/FidsLocation',
      urlDep:         'https://localhost:7248/api/FidsSpare',
      urlCountries:   'https://localhost:7248/api/Countries',
      urlArr:         'https://localhost:7248/api/Arrivals',
      urlFlight:      'https://localhost:7248/api/CheckinCounter',
      urlGate:        'https://localhost:7248/api/Gate',
      urlCheckinCounter: 'https://localhost:7248/api/CheckinCounter',

      numberfload: 3,
      timefresh: 65,
      timefloop: 25,
      numberfloadArr: 2,
      timefloopArr: 25,
      numberfloadDep2: 2,
      timefloopDep2: 25,
      timeCheckFlight: 50,
      timeLoadFlight: 60,
    }
  }
})
