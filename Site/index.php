<?php
    $ipaddress = '';

    if (getenv('HTTP_CLIENT_IP'))
        $ipaddress = getenv('HTTP_CLIENT_IP');
    else if(getenv('HTTP_X_FORWARDED_FOR'))
        $ipaddress = getenv('HTTP_X_FORWARDED_FOR');
    else if(getenv('HTTP_X_FORWARDED'))
        $ipaddress = getenv('HTTP_X_FORWARDED');
    else if(getenv('HTTP_FORWARDED_FOR'))
        $ipaddress = getenv('HTTP_FORWARDED_FOR');
    else if(getenv('HTTP_FORWARDED'))
       $ipaddress = getenv('HTTP_FORWARDED');
    else if(getenv('REMOTE_ADDR'))
        $ipaddress = getenv('REMOTE_ADDR');
    
    $lang = 'en';


    if ($ipaddress != ''){
    	$json = file_get_contents('http://api.ipapi.com/'.$ipaddress.'?access_key=7e701aaf2b565e00f2c96254385f44e0');
		$data = json_decode($json);

		if ($data->country_code == 'RU' || $data->country_code == 'UA')
			$lang = 'ru';
    }

    header('Location: /'.$lang);
?>