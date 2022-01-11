module.exports = async function (context, eventGridEvent) {
  context.log(typeof eventGridEvent);
  context.log(eventGridEvent);

  context.log(
    'EVGH_TripExternalizataions2CosmosDB triggered....EventGridEvent'
  );
  context.log('Subject: ' + eventGridEvent.subject);
  context.log('Time: ' + eventGridEvent.eventTime);
  context.log('Data: ' + JSON.stringify(eventGridEvent.data));

  context.bindings.outputDocument = JSON.stringify(eventGridEvent.data);

  context.done();

}
