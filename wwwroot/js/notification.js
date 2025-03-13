const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub", { withCredentials: true })
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

connection.keepAliveInterval = 15000;

connection.on("ReceiveNotification", function (message, url) {
    showNotification("서버에서 온 알림", message, url);
});

connection.start()
    .then(() => console.log("SignalR 연결 완료!"))
    .catch(err => console.error("SignalR 연결 실패:", err.toString()));

function showNotification(title, message, url) {
    let notificationContainer = document.getElementById("notification-container");

    if (!notificationContainer) {
        notificationContainer = document.createElement("div");
        notificationContainer.id = "notification-container";
        notificationContainer.style.position = "fixed";
        notificationContainer.style.bottom = "20px";
        notificationContainer.style.right = "20px";
        notificationContainer.style.zIndex = "1000";
        notificationContainer.style.display = "flex";
        notificationContainer.style.flexDirection = "column";
        notificationContainer.style.alignItems = "flex-end";
        document.body.appendChild(notificationContainer);
    }

    let notification = document.createElement("div");
    notification.className = "notification-popup";
    notification.style.background = "#333";
    notification.style.color = "#fff";
    notification.style.padding = "10px";
    notification.style.marginBottom = "5px";
    notification.style.borderRadius = "5px";
    notification.style.boxShadow = "0px 0px 10px rgba(0,0,0,0.3)";
    notification.style.cursor = "pointer";
    notification.style.transition = "opacity 0.5s ease-in-out";

    notification.innerHTML = `<strong>${title}</strong><br>${message}`;

    if (url) {
        notification.addEventListener("click", function () {
            window.location.href = url;
        });
    }

    notificationContainer.appendChild(notification);

    setTimeout(() => {
        notification.style.opacity = "0";
        setTimeout(() => notification.remove(), 500);
    }, 5000);
}
