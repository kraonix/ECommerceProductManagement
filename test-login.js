const puppeteer = require('puppeteer');
(async () => {
    try {
        console.log("Launching browser...");
        const browser = await puppeteer.launch();
        const page = await browser.newPage();
        
        page.on('console', msg => console.log('BROWSER LOG:', msg.text()));

        await page.goto('http://localhost:4200/login', {waitUntil: 'networkidle0'});

        await page.type('input[name="email"]', 'sachin@ecommerce.local');
        await page.type('input[name="password"]', 'password123');

        await page.click('button[type="submit"]');

        await page.waitForNavigation({waitUntil: 'networkidle0', timeout: 5000}).catch(e => console.log("Navigation timeout (this is fine if it shows error on page)"));
        
        console.log("Current URL: ", page.url());
        if (page.url().includes('dashboard')) {
            console.log("LOGIN VERIFIED SUCCESSFULLY!");
        }
        await browser.close();
    } catch(e) {
        console.log("TEST ERROR: ", e.message);
        process.exit(1);
    }
})();
