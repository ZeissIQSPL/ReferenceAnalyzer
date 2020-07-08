Feature: ReferenceAnalysis
    In order to know what reference to remove
    As a consumer of the library
    I want to know a number of actual references

@analysis
Scenario Outline: Counting references
    Given I have a solution <solution>
    When I run analysis for Project1
    Then number of references to <target> should be <references>

    Examples:
    | solution                   | target   | references |
    | no_references              | Project2 | 0          |
    | one_used_reference         | Project2 | 1          |
    | two_references_one_unused  | Project2 | 1          |
    | two_references_one_unused  | Project3 | 0          |
    | multiple_references_to_one | Project2 | 4          |

Scenario: Defined references
    Given I have a solution two_references_one_unused
    When I run analysis for Project1
    Then Referenced projects should be within defined references list

Scenario: NuGet reference
    Given I have a solution nuget_references
    When I run analysis for Project1
    Then Only xunit.assert should be in actual references
    And nunit.framework should not be in actual references
    And No diagnostics should be reported

Scenario: Analyzing all
    Given I have a solution two_references_one_unused
    When I run full analysis
    Then Reports for all three should be returned

Scenario: Solution with CLR project
    Given I Disable throwing on errors
    And I have a solution solution_with_cpp_cli
    When I run analysis for Project1
    Then Only xunit.assert should be in actual references
