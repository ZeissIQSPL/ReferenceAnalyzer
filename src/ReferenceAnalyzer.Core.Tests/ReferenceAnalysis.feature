Feature: ReferenceAnalysis
	In order to know what reference to remove
	As a consumer of the library
	I want to know a number of actual references

@analysis
Scenario Outline: Counting references
	Given I have a solution <solution>
	When I run analysis for <target>
	Then number of references to <target> should be <references>

	Examples:
	| solution                   | target   | references |
	| no_references              | Project2 | 0          |
	| one_used_reference         | Project2 | 1          |
	| two_references_one_unused  | Project2 | 1          |
	| two_references_one_unused  | Project3 | 0          |
	| multiple_references_to_one | Project2 | 4          |