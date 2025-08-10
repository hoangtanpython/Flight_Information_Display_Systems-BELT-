<template>
    <div class="header1">
        <div class="logo1">
            <img class="logoaht1" src="/img/logo/AHTLogo.png" alt="" title="">
        </div>
        <div class="text1">
            <img class="logoaht1" src="/img/logo/Icon-Arrival.png" alt="" title="">
            <p>ARRIVALS</p>
        </div>
        
    </div>
    
</template>

<script setup lang="ts">

</script>

<style>
.header1 {
    background-color: #121441;
    display: flex;
    flex-direction: row;
    align-items: center;
    /* padding: 2px 0.5rem; */
    height: 10vh;
    justify-content: space-between;
    width: 100vw;
}
.logoaht1 {
    max-height: 70%;
    width: auto;
}
.logo1 {
    height: 100%;
    width: 40vw;
    display: flex;
    align-items: center;
    justify-content: flex-start;
}
.text1 {
    color: white;
    font-weight: 700;
    font-size: large;
    /* margin-left: 22px; */
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