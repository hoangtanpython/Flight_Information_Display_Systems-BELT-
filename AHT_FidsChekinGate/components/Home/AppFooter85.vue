<template>
    <div class="footer1">
        <p class="date1">{{ date  }}</p>
    </div>
    
  </template>
  <script>
  import { format } from 'date-fns'
  export default  {
    data() {
      return {
        date: '',
      };
    },
    mounted() {
    setInterval(this.formattedDate, 1000);
    },
    methods: {
      formattedDate() {
      this.date =  format(new Date(), 'dd MMMM yyyy | HH:mm').toUpperCase()
    }
    },
    
  }
  </script>
  <style>
  .footer1 {
    background-color: #36c0c7;
    height: 4.5vh;
    display: flex;
    justify-content: flex-end;
    align-items: center;
    position: fixed;
    bottom: 0px;
    width: 100vw;
}
p.date1 {
    color: #121542;
    font-size: 2.5vh;
    padding-top: 0.5vh;
    padding-right: 3vw;
}
  </style>