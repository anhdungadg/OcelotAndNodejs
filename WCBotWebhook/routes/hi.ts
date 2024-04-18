import express = require('express');
const router = express.Router();

// Define a new route that returns the current system time as a JSON object
router.get('/time', (req: express.Request, res: express.Response) => {
    const currentTime = new Date().toISOString();
    res.json({ currentTime });
});

// Define the existing route that returns a string
router.get('/', (req: express.Request, res: express.Response) => {
    res.send("respond with a resource");
});

export default router;
