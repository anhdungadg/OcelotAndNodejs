"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const express = require("express");
const router = express.Router();
// Define a new route that returns the current system time as a JSON object
router.get('/time', (req, res) => {
    const currentTime = new Date().toISOString();
    res.json({ currentTime });
});
// Define the existing route that returns a string
router.get('/', (req, res) => {
    res.send("respond with a resource");
});
exports.default = router;
//# sourceMappingURL=hi.js.map