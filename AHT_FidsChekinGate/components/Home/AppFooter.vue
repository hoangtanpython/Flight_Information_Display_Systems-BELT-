<template>
  <div class="footer">
      <p class="date">{{ date  }}</p>
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
color: #121542;
padding-right: 4vw;
font-size: 4vh;
float: right;
padding-top: 1.8vh;
font-weight: 500;
}
</style>