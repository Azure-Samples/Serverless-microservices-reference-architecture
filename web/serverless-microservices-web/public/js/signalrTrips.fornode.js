const apiBaseUrl = 'http://localhost:7071';
//const hubName = 'simplechat';

getConnectionInfo().then(info => {
    var username;

    const messageForm = document.getElementById('message-form');
    const messageBox = document.getElementById('message-box');
    const messages = document.getElementById('messages');
    const options = {
        accessTokenFactory: () => info.accessKey
    };
    
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(info.endpoint, options)
        .configureLogging(signalR.LogLevel.Information)
        .build();
    connection.on('newMessage', (message) => {
        const newMessage = document.createElement('li');
        newMessage.appendChild(document.createTextNode(`${message.sender}: ${message.text}`));
        messages.appendChild(newMessage);
    });
    connection.onclose(() => console.log('disconnected'));
    console.log('connecting...');
    connection.start()
        .then(() => console.log('connected!'))
        .catch(console.error);
    messageForm.addEventListener('submit', ev => {
        ev.preventDefault();
        const message = messageBox.value;
        sendMessage(username, message);
        messageBox.value = '';
    });
}).catch(alert);

function getConnectionInfo() {
    return axios.post(`${apiBaseUrl}/api/negotiate`)
        .then(resp => resp.data);
}
function sendMessage(sender, messageText) {
    return axios.post(`${apiBaseUrl}/api/messages`, {
        sender: sender,
        text: messageText
    }).then(resp => resp.data);
}
