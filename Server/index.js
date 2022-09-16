const express = require('express');
const app = express();

app.use(express.json());

app.post('*', (req, res) => {
  console.log(req.body);
  res.send(req.body);
});

app.listen(3000);