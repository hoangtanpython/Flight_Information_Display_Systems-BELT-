<template>
    <img  alt="Vuelogo" :src="`/img/fullscreen/AHT_1920x1080.png?timestamp=${new Date().getTime()}`" />
</template>


<script setup lang="ts">
  definePageMeta({
  layout: 'checkinlayout' // Chỉ định layout custom
  })

  </script>