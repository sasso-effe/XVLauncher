<?php
	//get data from client
	$ver = $_POST['version'];

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

	$sql = "INSERT INTO Users (version) VALUES ('$ver')";

	if ($conn->query($sql) === TRUE) {
		$last_id = $conn->insert_id;
		echo "id=" . $last_id;
	} else {
		echo "Error: " . $sql . "<br>" . $conn->error;
	}
	$conn->close();
?>