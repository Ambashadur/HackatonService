rule simple_rules
{
    strings:
        $hex_string = "hello world"
        $anti_malware_string = "kkk1kkk2"

    condition:
        any of them
}