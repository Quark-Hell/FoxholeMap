const mapCanvas = document.getElementById("mapCanvas");
const mapViewport = document.getElementById("mapViewport");
const debugInfo = document.getElementById("debugInfo");

const mouse = new Mouse();

class MapButtons {
    constructor() {
        this.maxZoom = 6;
        this.minZoom = 0;
        
        this.zoom = 0.0;

        this.tileList = [];
        
        this.GetMap();
        this.addEvents();
    }

    renderTiles() {
        const rect = mapViewport.getBoundingClientRect();
        const screenWidth = rect.width;
        const screenHeight = rect.height;

        const centerX = screenWidth / 2;
        const centerY = screenHeight / 2;
        
        let html = '';
        for (const tile of this.tileList) {
            const top = tile.row * 256 + mouse.currentY;
            const left = tile.col * 256 + mouse.currentX;
            
            const isVisible =
                left + 256 > 0 && left < screenWidth &&
                top + 256 > 0 && top < screenHeight;

            if (!isVisible) continue;

            const url = `/MapAssets/Sat Tiles/${tile.zoom}/${tile.zoom}_${tile.col}_${tile.row}.png`;
            html += `<img class="map-tile" src="${url}" 
                     style="position:absolute; top:${top}px; left:${left}px; width:256px; height:256px;" />`;
        }

        mapCanvas.innerHTML = html;
    }
    
    GetMap(){
        fetch('/Map/GetTiles', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                Zoom: this.zoom,
                OffsetX: mouse.currentX,
                OffsetY: mouse.currentY
            })
        })
            .then(r => r.json())
            .then(data => {
                console.log("Tile data from server:", data);
                this.tileList = data;
                this.renderTiles()
            });
    }

    addEvents() {
        document.getElementById("zoomIn").addEventListener("click", () => {
            if (this.zoom !== this.maxZoom) {
                this.zoom++;
                const info = `X: ${mouse.currentX}, Y: ${mouse.currentY}, Zoom: ${this.zoom.toFixed(1)}`;
                debugInfo.textContent = info;

                this.GetMap();
            }
        });

        document.getElementById("zoomOut").addEventListener("click", () => {
            if (this.zoom !== this.minZoom) {
                this.zoom--;
                const info = `X: ${mouse.currentX}, Y: ${mouse.currentY}, Zoom: ${this.zoom.toFixed(1)}`;
                debugInfo.textContent = info;

                this.GetMap();
            }
        });
        
        document.getElementById("resetView").addEventListener("click", () => {
            mouse.currentX = 0;
            mouse.currentY = 0;
            this.zoom = 0;
            
            const info = `X: ${mouse.currentX}, Y: ${mouse.currentY}, Zoom: ${this.zoom.toFixed(1)}`;
            debugInfo.textContent = info;
            this.GetMap();
        })

        mapViewport.addEventListener("mousedown", (e) => {
            mouse.IsDragging = true;

            mouse.lastMouseX = e.clientX;
            mouse.lastMouseY = e.clientY;
        })

        addEventListener("mouseup", (e) => {
            mouse.IsDragging = false;

            mouse.lastMouseX = e.clientX;
            mouse.lastMouseY = e.clientY;
        })

        mapViewport.addEventListener("mousemove", (e) => {
            if(!mouse.IsDragging) return;

            const deltaX = e.clientX - mouse.lastMouseX;
            const deltaY = e.clientY - mouse.lastMouseY;

            mouse.lastMouseX = e.clientX;
            mouse.lastMouseY = e.clientY;

            const normalizedDeltaX = deltaX / mouse.screen.screenWidth * mouse.factor * (this.zoom + 1);
            const normalizedDeltaY = deltaY / mouse.screen.screenHeight * mouse.factor * (this.zoom + 1);

            mouse.currentX += normalizedDeltaX;
            mouse.currentY += normalizedDeltaY;

            const info = `
            X: ${mouse.currentX}, 
            Y: ${mouse.currentY}, 
            Zoom: ${this.zoom.toFixed(1)}`;

            debugInfo.textContent = info;
            this.renderTiles();
        })
    }
}

const mapButtons = new MapButtons();