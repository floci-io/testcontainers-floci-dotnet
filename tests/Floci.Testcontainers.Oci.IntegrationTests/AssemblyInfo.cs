using Xunit;

// These are container-backed integration tests: each test class starts its own Floci
// container (and some, like RDS, spawn additional sibling containers). Running them in
// parallel hammers the Docker daemon and causes flaky startup/timeout failures, so we
// run the suite serially. Reliability over speed for integration tests.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
