var vueApp;
var hubConnection;
var notificationSystem = {
  options: {
    info: {
      position: 'bottomRight'
    },
    success: {
      position: 'bottomRight'
    },
    warning: {
      position: 'bottomRight'
    },
    error: {
      position: 'topRight'
    }
  }
};

window.addEventListener("load", async e => {
  await connectToSignalRAsync();
});

getSignalRInfoAsync = async (url) => {
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
    return signalRInfo;
  } else {
    console.log(`getSignalRInfoAsync Response status: ${rawResponse.status}`);
    return null;
  }
}

connectToSignalRAsync = async () => {
  let singalrInfo = await getSignalRInfoAsync(window.singalrInfoUrl);
  if (singalrInfo != null) {
    let options = {
      accessTokenFactory: () => singalrInfo.accessKey
    };

    hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(singalrInfo.endpoint, options)
      .configureLogging(signalR.LogLevel.Information)
      .build();

    vueApp = window.App;

    hubConnection.on('tripUpdated', (trip) => {
      console.log(`tripUpdated Trip code: ${trip.code}`);
      vueApp.$toast.success(
        `tripUpdated for trip ${trip.code}`,
        'Trip Updated',
        notificationSystem.options.success
      );
    });

    hubConnection.on('tripDriversNotified', (trip) => {
      console.log(`tripDriversNotified Trip code: ${trip.code}`);
      vueApp.$toast.info(
        `tripDriversNotified for trip ${trip.code}`,
        'Drivers Notified',
        notificationSystem.options.info
      );
    });

    hubConnection.on('tripDriverPicked', (trip) => {
      console.log(`tripDriverPicked Trip code: ${trip.code}`);
      vueApp.$toast.info(
        `tripDriverPicked for trip ${trip.code}`,
        'Driver Picked',
        notificationSystem.options.info
      );
    });

    hubConnection.on('tripStarting', (trip) => {
      console.log(`tripStarting Trip code: ${trip.code}`);
      vueApp.$toast.info(
        `tripStarting for trip ${trip.code}`,
        'Trip Starting',
        notificationSystem.options.info
      );
    });

    hubConnection.on('tripRunning', (trip) => {
      console.log(`tripRunning Trip code: ${trip.code}`);
      vueApp.$toast.info(
        `tripRunning for trip ${trip.code}`,
        'Trip Running',
        notificationSystem.options.info
      );
    });

    hubConnection.on('tripCompleted', (trip) => {
      console.log(`tripCompleted Trip code: ${trip.code}`);
      vueApp.$toast.success(
        `tripCompleted for trip ${trip.code}`,
        'Trip Completed',
        notificationSystem.options.success
      );
    });

    hubConnection.on('tripAborted', (trip) => {
      console.log(`tripAborted Trip code: ${trip.code}`);
      vueApp.$toast.warning(
        `tripAborted for trip: ${trip.code}`,
        'Trip Aborted',
        notificationSystem.options.warning
      );
    });

    hubConnection.start().catch(err => console.log(err.toString()));
  }
}
