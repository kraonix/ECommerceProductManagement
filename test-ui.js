const puppeteer = require('puppeteer');
(async () => {
    try {
        const browser = await puppeteer.launch();
        const page = await browser.newPage();
        
        page.on('console', msg => console.log('BROWSER LOG:', msg.text()));
        page.on('pageerror', error => console.log('BROWSER EXCEPTION:', error.message));
        page.on('requestfailed', request => console.log('REQUEST FAILED:', request.url(), request.failure() ? request.failure().errorText : ''));

        await page.goto('http://localhost:4200/login', {waitUntil: 'networkidle0'});
        console.log("Navigated to login");

        await page.type('input[name="email"]', 'sachin@ecommerce.local');
        await page.type('input[name="password"]', 'admin123');
        console.log("Typed credentials");

        await page.click('button[type="submit"]');
        console.log("Clicked submit");

        await page.waitForTimeout(3000); // Wait 3 seconds to see logs
        console.log("Closing");
        
        await browser.close();
    } catch(e) {
        console.log("TEST JS ERROR: ", e.message);
    }
})();
