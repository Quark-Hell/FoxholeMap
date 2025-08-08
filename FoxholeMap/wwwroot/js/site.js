const mapCanvas = document.getElementById("mapCanvas");
const mapViewport = document.getElementById("mapViewport");
const debugInfo = document.getElementById("debugInfo");

let zoom = 0

class Screen {
    screenWidth = 0;
    screenHeight = 0;

    constructor() {
        const resizeObserver = new ResizeObserver(() => {
            this.updateInfo();
        });

        resizeObserver.observe(mapViewport);
        this.updateInfo();
    }

    updateInfo() {
        const rect = mapViewport.getBoundingClientRect();
        this.screenWidth = rect.width;
        this.screenHeight = rect.height;
        const info = `X: ${this.screenWidth}, Y: ${this.screenHeight}, Zoom: ${zoom.toFixed(1)}`;
        debugInfo.textContent = info;
    }
}

class Mouse {
    currentX = 0;
    currentY = 0;
    
    screen = new Screen();
    
    constructor() {
        this.lastMouseX = 0;
        this.lastMouseY = 0;

        this.factor = 10;

        this.IsDragging = false;
        
        this.addEvents();
        this.screen = new Screen();
    }
    
    addEvents(){
        mapViewport.addEventListener("mousedown", (e) => {
            this.IsDragging = true;

            this.lastMouseX = e.clientX;
            this.lastMouseY = e.clientY;
        })

        addEventListener("mouseup", (e) => {
            this.IsDragging = false;

            this.lastMouseX = e.clientX;
            this.lastMouseY = e.clientY;
        })

        mapViewport.addEventListener("mousemove", (e) => {
            if(!this.IsDragging) return;

            const deltaX = e.clientX - this.lastMouseX;
            const deltaY = e.clientY - this.lastMouseY;

            this.lastMouseX = e.clientX;
            this.lastMouseY = e.clientY;

            const normalizedDeltaX = deltaX / this.screen.screenWidth * this.factor;
            const normalizedDeltaY = deltaY / this.screen.screenHeight * this.factor;

            this.currentX += normalizedDeltaX;
            this.currentY += normalizedDeltaY;

            const info = `
            X: ${this.currentX}, 
            Y: ${this.currentY}, 
            Zoom: ${zoom.toFixed(1)}`;
            
            debugInfo.textContent = info;
        })
    }
}

const mouse = new Mouse();

document.getElementById("zoomIn").addEventListener("click", () => {
    fetch("/Map/UpscaleMap", {
        method: "POST"
    })
        .then(response => {
            if (!response.ok) throw new Error(`HTTP error ${response.status}`);
            return response.json();
        })
        .then(updatedViewport => {
            zoom = updatedViewport.zoom;

            const info = `X: ${mouse.currentX}, Y: ${mouse.currentY}, Zoom: ${zoom.toFixed(1)}`;
            debugInfo.textContent = info;
        })
        .catch(error => console.error("Ошибка зума:", error));
});

document.getElementById("zoomOut").addEventListener("click", () => {
    fetch("/Map/DownscaleMap", {
        method: "POST"
    })
        .then(response => {
            if (!response.ok) throw new Error(`HTTP error ${response.status}`);
            return response.json();
        })
        .then(updatedViewport => {
            zoom = updatedViewport.zoom;

            const info = `X: ${mouse.currentX}, Y: ${mouse.currentY}, Zoom: ${zoom.toFixed(1)}`;
            debugInfo.textContent = info;
        })
        .catch(error => console.error("Ошибка зума:", error));
});

document.getElementById("resetView").addEventListener("click", () => {
    fetch("/Map/ResetMap", {
        method: "POST"
    })
    .then(response => {
        if (!response.ok) throw new Error(`HTTP error ${response.status}`);
        return response.json();
    })
    .then(updatedViewport => {
        zoom = updatedViewport.zoom;
        mouse.currentX = updatedViewport.zoom;
        mouse.currentY = updatedViewport.zoom;

        const info = `X: ${mouse.currentX}, Y: ${mouse.currentY}, Zoom: ${zoom.toFixed(1)}`;
        debugInfo.textContent = info;
    })
    .catch(error => console.error("<UNK> <UNK>:", error));
})