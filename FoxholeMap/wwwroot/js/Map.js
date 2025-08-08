document.getElementById("zoomIn").addEventListener("click", () => {
    fetch("/Map/UpscaleMap", {
        method: "POST"
    })
        .then(response => {
            if (!response.ok) throw new Error(`HTTP error ${response.status}`);
            return response.json();
        })
        .then(updatedViewport => {
            console.log("Updated viewport:", updatedViewport);
            
            const info = `X: ${updatedViewport.offsetX}, Y: ${updatedViewport.offsetY}, Zoom: 5.0`;
            document.getElementById("debugInfo").textContent = info;
        })
        .catch(error => console.error("Ошибка зума:", error));
});