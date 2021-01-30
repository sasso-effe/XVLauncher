<?php
	//get data from client
	$ver = $_POST['version'];
	$id = $_POST['id'];
	$error = $_POST['error'];

	// TODO: insert your credentials
	$dbhost = '';
	$dbname = '';
	$dbase = '';
	$psw = '';
	// Create connection
	$conn = mysqli_connect($dbhost,$dbname,$psw,$dbase);
	// Check connection
	if ($conn->connect_error) {
		die("Connection failed: " . $conn->connect_error);
	}

	$sql = "UPDATE Users SET version = '$ver', last_error = '$error', error_date = now() WHERE id = $id";
	if ($conn->query($sql) === TRUE) {
		$last_id = $conn->insert_id;
		echo "version updated";
	} else {
		echo "Error: " . $sql . "<br>" . $conn->error;
	}
	$conn->close();
?>