<template>
    <div class="header">
        <div class="logo">
            <img class="logoaht" src="/img/logo/AHTLogo.png" alt="" title="">
        </div>
        <div class="text">
            <img class="logoaht" src="/img/logo/Icon-Departure.png" alt="" title="">
            <p>DEPARTURES</p>
        </div>
        
    </div>
    
</template>

<script setup lang="ts">

</script>

<style>
.header {
    background-color: #121441 !important;
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
    width: 40vw;
}
.text {
    color: white !important;
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
    color: white;
    font-weight: 700;
    font-size: 7vw;
    /* padding-top: 7%; */
}


</style>