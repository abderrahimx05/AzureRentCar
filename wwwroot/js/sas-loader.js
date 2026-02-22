// Load SAS URLs for Azure images on-the-fly
// This script automatically converts Azure Blob URLs to secure SAS URLs

document.addEventListener('DOMContentLoaded', async function() {
    const images = document.querySelectorAll('img[src*="azurrentcar.blob.core.windows.net"]');
    
    if (images.length === 0) return;
    
    console.log(`Found ${images.length} Azure images to secure with SAS tokens`);
    
    for (const img of images) {
        const originalUrl = img.src;
        if (originalUrl.includes('?')) {
            // Already has parameters (likely SAS token), skip
            continue;
        }
        
        try {
            const response = await fetch(`/api/image-url?url=${encodeURIComponent(originalUrl)}`);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            
            const data = await response.json();
            if (data.url && data.url !== originalUrl) {
                const oldUrl = img.src;
                img.src = data.url;
                console.log(`✓ Secured image with SAS token`);
            }
        } catch (error) {
            console.warn('Failed to generate SAS URL for image:', originalUrl, error);
            // Keep original URL if SAS generation fails
        }
    }
});
