const express = require('express');
const app = express();

app.listen(3000, () => console.log('Example app listening on port 3000!'));

app.use('/static', express.static(path.join(__dirname, 'static')));