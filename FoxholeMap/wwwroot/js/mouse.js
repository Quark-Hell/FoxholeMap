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
    }
}

class Mouse {
    currentX = 0;
    currentY = 0;

    screen = new Screen();

    constructor() {
        this.lastMouseX = 0;
        this.lastMouseY = 0;

        this.factor = 50;

        this.IsDragging = false;
        
        this.screen = new Screen();
    }
}