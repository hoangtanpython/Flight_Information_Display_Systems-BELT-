<template>
<div class="">
    <div class="card-body pad table-responsive" style="width: 100%; overflow: hidden;">
        <div class="row">
            <div class="col-12" style="margin-top: 10px;">
            <!-- split buttons box -->
            <!-- Horizontal Form -->
            <div class="card card-info">
              <div class="cardheader">
                <h3 class="cardtitle" >Flight Information</h3>
              </div>
              <!-- /.card-header -->
              <!-- form start -->
              <form class="form-horizontal" @submit.prevent="saveData">
                <div class="card-body">
                  <div class="form-group row styleFromcontrol" >
                    <label for="inputEmail3" class="col-sm-4 col-form-label">FlightNo</label>
                    <div class="col-sm-8" style="display: flex;align-items: center;">
                      <spam style="font-weight: bold;">{{ localFlightData.flight ?? '' }}</spam>
                    </div>
                  </div>
                  <div class="form-group row styleFromcontrol">
                    <label for="inputPassword3" class="col-sm-4 col-form-label">Scheduled</label>
                    <div class="col-sm-8" style="display: flex;align-items: center;">
                        <spam style="font-weight: bold;">{{ localFlightData.schedule ?? '' }}</spam>
                    </div>
                  </div>
                  <div class="form-group row styleFromcontrol" >
                    <label for="inputPassword3" class="col-sm-4 col-form-label">Counters</label>
                    <div class="col-sm-8" style="display: flex;align-items: center;">
                      <input class="form-control classInput" type="text"
                      v-model="localFlightData.checkInCounters" >
                    </div>
                  </div>
                  <div class="form-group row styleFromcontrol" >
                    <label for="inputPassword3" class="col-sm-4 col-form-label">CounterStart</label>
                    <div class="col-sm-8" style="display: flex;align-items: center;">
                      <input class="form-control classInput" type="text"
                      v-model="localFlightData.counterStart" >
                    </div>
                  </div>
                  <div class="form-group row " >
                    <label for="inputPassword3" class="col-sm-4 col-form-label">CounterEnd</label>
                    <div class="col-sm-8" style="display: flex;align-items: center;">
                      <input class="form-control classInput" type="text"
                      v-model="localFlightData.counterEnd" >
                    </div>
                  </div>
                  <div class="form-group row styleFromcontrol" >
                    <label for="inputPassword3" class="col-sm-4 col-form-label">Gate</label>
                    <div class="col-sm-8" style="display: flex;align-items: center;">
                      <input class="form-control classInput" type="text"
                      v-model="localFlightData.gate" >
                    </div>
                  </div>
                  <div class="form-group row styleFromcontrol" >
                    <label for="inputPassword3" class="col-sm-4 col-form-label">GateStart</label>
                    <div class="col-sm-8" style="display: flex;align-items: center;">
                      <input class="form-control classInput" type="text"
                      v-model="localFlightData.gateStart" >
                    </div>
                  </div>
                  <div class="form-group row styleFromcontrol" >
                    <label class="col-sm-4 col-form-label">GateEnd</label>
                    <div class="col-sm-8" style="display: flex;align-items: center;">
                      <input class="form-control classInput" type="text"
                      v-model="localFlightData.gateEnd" >
                    </div>
                  </div>
                  <div class="form-group row styleFromcontrol" >
                    <label class="col-sm-4 col-form-label">Actual</label>
                    <div class="col-sm-8" style="display: flex;align-items: center;">
                      <input class="form-control classInput" type="text"
                      v-model="localFlightData.actual" >
                    </div>
                  </div>
                  <div class="form-group row styleFromcontrol" v-if="selectedStatus == 'Delayed'">
                    <label class="col-sm-4 col-form-label">Estimated</label>
                    <div class="col-sm-8" style="display: flex;align-items: center;">
                      <input class="form-control classInput" type="text"
                      v-model="localFlightData.estimated" >
                    </div>
                  </div>
                  <div class="form-group row styleFromcontrol" >
                    <label class="col-sm-4 col-form-label">Status</label>
                    <div class="col-sm-8" style=" display: flex;align-items: center;">
                        <select class="form-control classInput" v-model="selectedStatus">
                          <option>On time</option>
                          <option>Departed</option>
                          <option>Delayed</option>
                          <option>Canceled</option>
                          <option>Gate closed</option>
                        </select>
                    </div>
                  </div>


                </div>
                <!-- /.card-body -->
                <div class="card-footer">
                  <button type="submit" class="btnn btn-info" >Save</button>
                  <button type="button" class="btnn btn-default float-right" @click="cancelarray" style="color: black;">Cancel</button>
                </div>
                <!-- /.card-footer -->
              </form>
            </div>
            <!-- /.card -->
            </div>
        </div>
        <!--End-->
    </div>
    <!-- /.card -->
</div>
</template>

<script setup lang="ts">
interface FlightData  {
      id: string,
      scheduledDate: string,
      schedule: string,
      estimated: string,
      actual: string,
      lineCode: string,
      flight: string,
      city: string,
      gate: string,
      remark: string,
      status: string,
      rowFrom: string,
      rowTo: string,
      checkInCounters: string,
      counterStart: string,
      counterEnd: string, 
      gateStart: string,
      gateEnd:string
};

const props = defineProps<{
  flightData: FlightData;
}>();

const selectedStatus = ref(props.flightData.status); 


const convertToTime = (datetime: string) => {
  const date = new Date(datetime);
  return date.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' }); // Định dạng HH:mm
};

const timeStart = ref(convertToTime(props.flightData.counterEnd));
const timeEnd = ref(convertToTime(props.flightData.counterEnd));
const localFlightData = reactive(JSON.parse(JSON.stringify(props.flightData)));


watch(() => props.flightData, (newVal) => {
    selectedStatus.value = props.flightData.status;
    //timeStart.value = convertToTime(props.flightData.counterStart);
    timeEnd.value = convertToTime(props.flightData.counterEnd);
    Object.assign(localFlightData, newVal); // Cập nhật lại các giá trị mới vào localFlightData
}, { deep: true });

const emit = defineEmits<{
  (e: 'result', updatedData: FlightData): void;
}>();

function saveData() {
  localFlightData.status = selectedStatus.value;
  localFlightData.mcdt = localFlightData.mcdt == null ? "" : localFlightData.mcdt;
  console.log(localFlightData.mcdt);
  emit('result', localFlightData);
  resetFlightData(localFlightData);
  selectedStatus.value = "";
  
  //console.log('Dữ liệu đã lưu:', localFlightData);
};

function resetFlightData(data: FlightData): void {
  // Duyệt qua tất cả các thuộc tính của đối tượng `data`
  for (const key in data) {
    if (data.hasOwnProperty(key)) {
      data[key as keyof FlightData] = ""; // Đặt giá trị của mỗi thuộc tính thành ""
    }
  }
};

const cancelarray =() =>
{
  resetFlightData(localFlightData);
  selectedStatus.value = "";
};

const GettimeStart =(date: string) :string => {
  return date;
}
</script>


<style>
.classInput{
    height: 30px;
}

.form-group.row.styleFromcontrol{
    margin-bottom: 0.2rem !important;
}

.cardheader {
    border-bottom: 1px solid rgba(0, 0, 0, .125);
    padding: 0.35rem 1.25rem;
    position: relative;
    border-top-left-radius: .25rem;
    border-top-right-radius: .25rem;
    background-color: #1accd1;
    padding-bottom: 0px;
}
label.col-sm-4.col-form-label {
    font-weight: bold;
    color: black;
}

.btnn {
    text-align: center;
    vertical-align: middle;
    border: 1px solid transparent;
    padding: .375rem .75rem;
    font-size: 1rem;
    line-height: 1.5;
    border-radius: .25rem;
}
.cardtitle {
    margin-bottom: 0.3rem;
    color: white;
    font-size: x-large;
}
</style>