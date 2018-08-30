module.exports = function (context, req) {
  context.bindings.signalRMessages = [{
    "target": "newMessage",
    "arguments": [ req.body ]
  }];
  context.done();
};