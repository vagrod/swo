<?php
// Configure your Subject Prefix and Recipient here
$subjectPrefix = 'SWO Support Request';
$emailTo       = 'support@swo.su';
$errors = array(); // array to hold validation errors
$data   = array(); // array to pass back data

if($_SERVER['REQUEST_METHOD'] === 'POST') {
    $name    = stripslashes(trim($_POST['author']));
    $email   = stripslashes(trim($_POST['email']));
    $message = stripslashes(trim($_POST['comment']));
    if (empty($name)) {
        $errors['author'] = 'Name is required.';
    }
    if (!filter_var($email, FILTER_VALIDATE_EMAIL)) {
        $errors['email'] = 'Email is invalid.';
    }
    if (empty($message)) {
        $errors['comment'] = 'Message is required.';
    }
    // if there are any errors in our errors array, return a success boolean or false
    if (!empty($errors)) {
        $data['success'] = false;
        $data['errors']  = $errors;

        header('Location: /en/contacts.html');
    } else {
        $subject = "$subjectPrefix $subject";
        $body    = '
            <strong>Name: </strong>'.$name.'<br />
            <strong>Email: </strong>'.$email.'<br />
            <strong>Message: </strong>'.nl2br($message).'<br />
        ';
        $headers  = "MIME-Version: 1.1" . PHP_EOL;
        $headers .= "Content-type: text/html; charset=utf-8" . PHP_EOL;
        $headers .= "Content-Transfer-Encoding: 8bit" . PHP_EOL;
        $headers .= "Date: " . date('r', $_SERVER['REQUEST_TIME']) . PHP_EOL;
        $headers .= "Message-ID: <" . $_SERVER['REQUEST_TIME'] . md5($_SERVER['REQUEST_TIME']) . '@' . $_SERVER['SERVER_NAME'] . '>' . PHP_EOL;
        $headers .= "From: " . "=?UTF-8?B?".base64_encode($name)."?=" . "<$email>" . PHP_EOL;
        $headers .= "Return-Path: $emailTo" . PHP_EOL;
        $headers .= "Reply-To: $email" . PHP_EOL;
        $headers .= "X-Mailer: PHP/". phpversion() . PHP_EOL;
        $headers .= "X-Originating-IP: " . $_SERVER['SERVER_ADDR'] . PHP_EOL;
        mail($emailTo, "=?utf-8?B?" . base64_encode($subject) . "?=", $body, $headers);
        $data['success'] = true;
        $data['message'] = 'Congratulations. Your message has been sent successfully';

        header('Location: /en');
    }
    // return all our data to an AJAX call
    echo json_encode($data);
}
