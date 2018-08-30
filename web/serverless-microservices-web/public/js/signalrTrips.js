let hubConnection = {};

window.onload = function () {
  connectToSignalR();
};

var getSignalRInfoAsync = async (url) => {
  console.log(`SignalR Info URL ${url}`);
  const rawResponse = await fetch(url, {
    method: 'GET', // *GET, POST, PUT, DELETE, etc.
    mode: 'cors', // no-cors, cors, *same-origin
    cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
    credentials: 'same-origin', // include, same-origin, *omit
    headers: {
      'Content-Type': 'application/json; charset=utf-8'
    },
    redirect: 'follow', // manual, *follow, error
    referrer: 'no-referrer' // no-referrer, *client
  });

  if (rawResponse.status === 200) {
    let signalRInfo = await rawResponse.json();
    console.log(`Connection Endpoint: ${signalRInfo.endpoint}`);
    // this.$toast.success(
    //   `Connection Endpoint: ${signalRInfo.endpoint}`,
    //   'Success',
    //   this.notificationSystem.options.success
    // );
    return signalRInfo;
  } else {
    console.log(`getSignalRInfoAsync Response status: ${rawResponse.status}`);
    // this.$toast.warning(
    //   `Connection Failure: ${rawResponse.status}`,
    //   'Error',
    //   this.notificationSystem.options.success
    // );
    return null;
  }
}

function connectToSignalR() {
  let singalrInfo = getSignalRInfoAsync(window.singalrInfoUrl);
  if (singalrInfo != null) {
    let options = {
      accessTokenFactory: () => singalrInfo.accessKey
    };

    hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(singalrInfo.endpoint, options)
      .configureLogging(signalR.LogLevel.Information)
      .build();

    hubConnection.on('tripUpdated', (trip) => {
      console.log(`tripUpdated Trip code: ${trip.code}`);
      // this.$toast.success(
      //   `Trip code: ${trip.code}`,
      //   'Trip Updated',
      //   this.notificationSystem.options.success
      // );
    });

    hubConnection.on('tripDriversNotified', (trip) => {
      console.log(`tripDriversNotified Trip code: ${trip.code}`);
      // this.$toast.success(
      //   `Trip code: ${trip.code}`,
      //   'tripDriversNotified',
      //   this.notificationSystem.options.success
      // );
    });

    hubConnection.on('tripDriverPicked', (trip) => {
      console.log(`tripDriverPicked Trip code: ${trip.code}`);
      // this.$toast.success(
      //   `Trip code: ${trip.code}`,
      //   'tripDriverPicked',
      //   this.notificationSystem.options.success
      // );
    });

    hubConnection.on('tripStarting', (trip) => {
      console.log(`tripStarting Trip code: ${trip.code}`);
      // this.$toast.success(
      //   `Trip code: ${trip.code}`,
      //   'tripStarting',
      //   this.notificationSystem.options.success
      // );
    });

    hubConnection.on('tripRunning', (trip) => {
      console.log(`tripRunning Trip code: ${trip.code}`);
      // this.$toast.success(
      //   `Trip code: ${trip.code}`,
      //   'tripRunning',
      //   this.notificationSystem.options.success
      // );
    });

    hubConnection.on('tripCompleted', (trip) => {
      console.log(`tripCompleted Trip code: ${trip.code}`);
      // this.$toast.success(
      //   `Trip code: ${trip.code}`,
      //   'tripCompleted',
      //   this.notificationSystem.options.success
      // );
    });

    hubConnection.on('tripAborted', (trip) => {
      console.log(`tripAborted Trip code: ${trip.code}`);
      // this.$toast.warning(
      //   `Trip code: ${trip.code}`,
      //   'tripAborted',
      //   this.notificationSystem.options.success
      // );
    });

    hubConnection.start().catch(err => console.log(err.toString()));
  }
}
