$.get( "/ns")
.done(function( data ){
  var workspace = data.ns;
  console.log(workspace);
  // subscribe to QueryTaskGraph
  var query_task_graph = new ROSLIB.Service({
    ros: ros,
    name: workspace + '/query_task_graph',
    serviceType: 'angel_msgs/srv/QueryTaskGraph'
  });

  var request = {};
  var task_list;
  query_task_graph.callService(request, function(result){
    // Load title
    var task_title = result.task_title;
    var title_container_block = document.getElementById('task_title');
    title_container_block.innerHTML = task_title;

    // Load tasks
    task_list = result.task_graph.task_steps;
    var task_levels = result.task_graph.task_levels;
    var container_block = document.getElementById('task-list');

    task_list.forEach(function(task, index){
      // TODO: support different task levels
      var task_level = task_levels[index];

      var task_line = document.createElement('div');
      task_line.className = "task-line";

      var checkbox = document.createElement('span');
      checkbox.className = "checkbox";
      checkbox.id = task;
      task_line.appendChild(checkbox);

      var checkmark = document.createElement('span');
      checkmark.className = "checkmark_hidden checkmark";
      checkmark.id = "checkmark";
      checkbox.appendChild(checkmark);

      var text = document.createElement('span');
      text.className = "text body-text task";
      text.innerHTML = task;
      task_line.appendChild(text);

      container_block.appendChild(task_line);

    });
  });

  // Create a listener for task completion updates
  var task_update = new ROSLIB.Topic({
    ros : ros,
    name : workspace + '/TaskUpdates',
    messageType : 'angel_msgs/msg/TaskUpdate' // find: $ ros2 topic type <>
  });

  task_update.subscribe(function(m) {
    // Update checkmarks
    var task_name = m.previous_step;
    var task_idx = task_list.indexOf(task_name);

    task_list.forEach(function(task, index){
      var el = document.getElementById(task);

      if (index <= task_idx){
        // Add checkmark to all tasks up to and including task
        el.querySelector('.checkmark').className = 'checkmark_visible checkmark';
      }
      else{
        // Remove checkmarks for all tasks after the task
        el.querySelector('.checkmark').className = 'checkmark_hidden checkmark';
      } 
    });

    // Update colors in chart
    var chart = Chart.getChart('activity-conf');
    var colors = new Array(chart.data.labels.length).fill("rgba(0, 104, 199, 1.0)");
    var idx = chart.data.labels.indexOf(task_name);

    for(var i=0; i<=idx; i++){
      colors[i] = "rgba(62, 174, 43, 1.0)"; // green
    }
    if(idx+1 < colors.length){
      colors[idx+1] = "rgb(254, 219, 101)"; // yellow
    }
    colors[chart.data.labels.indexOf("Background")] = "rgba(0, 104, 199, 1.0)"; // blue

    chart.data.datasets[0].backgroundColor = colors;

    chart.update('none');
  });

  // Done button
  function done() {
    // TODO: Update this
    console.log("Done!");
  }

  var done_btn = document.getElementById("done-btn");
  done_btn.onclick = done;
});