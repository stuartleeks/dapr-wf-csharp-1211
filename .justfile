default:
	just --list


############################################################################
# recipes for submitting and watching jobs

submit-job-simple:
	echo '{"steps": [{"name": "simple_test","actions" : [{"name": "simple_action", "action": "processor1","content" : "Hello World"}]}]}'  \
	| ./scripts/run-workflow.sh

submit-job-multi-step:
	echo '{"steps": [{"name": "parallel_step","actions" : [{"name": "action1.1", "action": "processor1","content" : "Hello World"},{"name": "action1.2", "action": "processor1","content" : "Do stuff"},{"name": "action1.3", "action": "processor1","content" : "Do more stuff"}]},{"name": "final_step","actions" : [{"name": "action2.1", "action": "processor1","content" : "Finale"}]}]}'  \
	| ./scripts/run-workflow.sh
