# Conventions

## Coding

- Prefer structuring code together by functionality rather than by layers.
- Add .NET doc comments to all public methods and properties.
- Prefer functional style for internal logic classes, pushing mutability to the outside.

## Testing

- Prefer a TDD flow. Write tests before implementing. Confirm the test code before proceeding.
- Minimise the knowledge of the tests about the internals of the code. Preferably the tests should only know about the API of the class they are testing.
- Acceptance tests test features. Found in *tests/Anu.Jobs.Tests.Acceptance/*.
- Building tests are used in a TDD fashion and can know more about the structure of the code. They can be deleted if the structure needs to change. Found in *tests/Anu.Jobs.Tests.Building/*.
- Communication tests test the contracts between different systems like external applications, databases, etc. Contract tests are a preferred example of this. Found in *tests/Anu.Jobs.Tests.Communication/*.
- The test class usually indicates the System Under Test (SUT). Tests are usually named `WhenX_ThenY`.
