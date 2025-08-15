class Crypto {
    constructor(chatID) {
        this.key = null;
    }

    async loadKey(chatID) {
        const response = await fetch('/Chat/GetDailyKey', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ chatID: chatID })
        });
        const data = await response.json();

        this.key = CryptoJS.enc.Base64.parse(data.chatKey);

        return this.key;
    }

    encryptMessage(message) {
        const iv = CryptoJS.lib.WordArray.random(16);

        const encrypted = CryptoJS.AES.encrypt(
            CryptoJS.enc.Utf8.parse(message),
            this.key,
            {
                iv: iv,
                mode: CryptoJS.mode.CBC,
                padding: CryptoJS.pad.Pkcs7
            }
        );

        const ivBase64 = CryptoJS.enc.Base64.stringify(iv);
        const cipherBase64 = CryptoJS.enc.Base64.stringify(encrypted.ciphertext);

        return ivBase64 + ":" + cipherBase64;
    }

    decryptMessage(cipherText) {
        const parts = cipherText.split(":");
        if (parts.length !== 2) throw new Error("Invalid encrypted data format");

        const iv = CryptoJS.enc.Base64.parse(parts[0]);
        const cipherBytes = CryptoJS.enc.Base64.parse(parts[1]);

        const decrypted = CryptoJS.AES.decrypt(
            { ciphertext: cipherBytes },
            this.key,
            {
                iv: iv,
                mode: CryptoJS.mode.CBC,
                padding: CryptoJS.pad.Pkcs7
            }
        );

        return CryptoJS.enc.Utf8.stringify(decrypted);
    }
}

class Chat {
    constructor() {
        this.chatID = 1;
        this.crypto = new Crypto(this.chatID);

        this.messagesContainer = document.getElementById("messages-container");
        this.textInput = document.getElementById("text-input");
        this.sendButton = document.getElementById("send-button");

        this.initialize();
    }

    async initialize() {
        await this.crypto.loadKey(this.chatID);  // ждём загрузки ключа!

        this.readChat();
        this.addSendMessageEvent();
    }

    addMessage(text, isOwn) {
        const messsage = this.generateHtmlMessage(text, isOwn);
        this.messagesContainer.innerHTML += messsage;
    }

    sendMessage() {
        this.addMessage(this.textInput.value,true);
        this.textInput.value = ""
        this.textInput.placeholder = "Введите сообщение..."
    }

    generateHtmlMessage(content, isOwn) {
        let message = "";

        if (isOwn)
        {
            message = "<div class=\"message own\">\n";
        }
        else
        {
            message = "<div class=\"message\">\n";
        }

        message +=
            "<div class=\"message-avatar non-selectable\">\n" +
            "<img src=\"/MapAssets/UI_Icons/bin.png\" alt=\"Вы\">\n" +
            "</div>\n" +
            "<div class=\"message-content\">\n" +
            content +
            "</div>\n" +
            "</div>";

        return message;
    }

    readChat(){
        fetch('/Chat/ReadChat', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ chatID: this.chatID, username: "Admin" })
        })
            .then(response => response.json())
            .then(data => {
                data.chat.forEach(item => {
                    const decrypted = this.crypto.decryptMessage(item._message);
                    this.addMessage(decrypted, item._isOwn);
                });
            });
    }

    addSendMessageEvent(){
        this.sendButton.addEventListener("click", async (e) => {
            if (!this.textInput.value.trim()) return;
            const encrypted = this.crypto.encryptMessage(this.textInput.value);

            const res = await fetch('/Chat/SendMessage', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({
                    ChatID: this.chatID,
                    UserName: "Admin",
                    Message: encrypted
                })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        this.sendMessage();
                    } else {
                        alert(data.error);
                    }
                });
        })
    }
}

const chat = new Chat();