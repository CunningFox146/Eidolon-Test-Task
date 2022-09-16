const express = require('express');
const app = express();

app.use(express.json());

app.post('*', (req, res) => {
  console.log(req.body);
  res.send(200);
});

app.listen(3000);