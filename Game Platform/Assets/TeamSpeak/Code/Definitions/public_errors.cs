class public_errors {
	//general
	public const uint ERROR_ok                                     = 0x0000;
	public const uint ERROR_undefined                              = 0x0001;
	public const uint ERROR_not_implemented                        = 0x0002;
	public const uint ERROR_ok_no_update                           = 0x0003;
	public const uint ERROR_dont_notify                            = 0x0004;
	public const uint ERROR_lib_time_limit_reached                 = 0x0005; 
	
	//dunno
	public const uint ERROR_command_not_found                      = 0x0100;
	public const uint ERROR_unable_to_bind_network_port            = 0x0101;
	public const uint ERROR_no_network_port_available              = 0x0102;
	
	//client
	public const uint ERROR_client_invalid_id                      = 0x0200;
	public const uint ERROR_client_nickname_inuse                  = 0x0201;
	public const uint ERROR_client_protocol_limit_reached          = 0x0203;
	public const uint ERROR_client_invalid_type                    = 0x0204;
	public const uint ERROR_client_already_subscribed              = 0x0205;
	public const uint ERROR_client_not_logged_in                   = 0x0206;
	public const uint ERROR_client_could_not_validate_identity     = 0x0207;
	public const uint ERROR_client_version_outdated                = 0x020a;
	public const uint ERROR_client_is_flooding                     = 0x020c;
	public const uint ERROR_client_hacked                          = 0x020d;
	public const uint ERROR_client_cannot_verify_now               = 0x020e;
	public const uint ERROR_client_login_not_permitted             = 0x020f;
	public const uint ERROR_client_not_subscribed                  = 0x0210;
	
	//channel
	public const uint ERROR_channel_invalid_id                     = 0x0300;
	public const uint ERROR_channel_protocol_limit_reached         = 0x0301;
	public const uint ERROR_channel_already_in                     = 0x0302;
	public const uint ERROR_channel_name_inuse                     = 0x0303;
	public const uint ERROR_channel_not_empty                      = 0x0304;
	public const uint ERROR_channel_can_not_delete_default         = 0x0305;
	public const uint ERROR_channel_default_require_permanent      = 0x0306;
	public const uint ERROR_channel_invalid_flags                  = 0x0307;
	public const uint ERROR_channel_parent_not_permanent           = 0x0308;
	public const uint ERROR_channel_maxclients_reached             = 0x0309;
	public const uint ERROR_channel_maxfamily_reached              = 0x030a;
	public const uint ERROR_channel_invalid_order                  = 0x030b;
	public const uint ERROR_channel_no_filetransfer_supported      = 0x030c;
	public const uint ERROR_channel_invalid_password               = 0x030d;
	public const uint ERROR_channel_invalid_security_hash          = 0x030f; //note 0x030e is defined in public_rare_errors;
	
	//server
	public const uint ERROR_server_invalid_id                      = 0x0400;
	public const uint ERROR_server_running                         = 0x0401;
	public const uint ERROR_server_is_shutting_down                = 0x0402;
	public const uint ERROR_server_maxclients_reached              = 0x0403;
	public const uint ERROR_server_invalid_password                = 0x0404;
	public const uint ERROR_server_is_virtual                      = 0x0407;
	public const uint ERROR_server_is_not_running                  = 0x0409;
	public const uint ERROR_server_is_booting                      = 0x040a;
	public const uint ERROR_server_status_invalid                  = 0x040b;
	public const uint ERROR_server_version_outdated                = 0x040d;
	public const uint ERROR_server_duplicate_running               = 0x040e;
	
	//parameter
	public const uint ERROR_parameter_quote                        = 0x0600;
	public const uint ERROR_parameter_invalid_count                = 0x0601;
	public const uint ERROR_parameter_invalid                      = 0x0602;
	public const uint ERROR_parameter_not_found                    = 0x0603;
	public const uint ERROR_parameter_convert                      = 0x0604;
	public const uint ERROR_parameter_invalid_size                 = 0x0605;
	public const uint ERROR_parameter_missing                      = 0x0606;
	public const uint ERROR_parameter_checksum                     = 0x0607;
	
	//unsorted, need further investigation
	public const uint ERROR_vs_critical                            = 0x0700;
	public const uint ERROR_connection_lost                        = 0x0701;
	public const uint ERROR_not_connected                          = 0x0702;
	public const uint ERROR_no_cached_connection_info              = 0x0703;
	public const uint ERROR_currently_not_possible                 = 0x0704;
	public const uint ERROR_failed_connection_initialisation       = 0x0705;
	public const uint ERROR_could_not_resolve_hostname             = 0x0706;
	public const uint ERROR_invalid_server_connection_handler_id   = 0x0707;
	public const uint ERROR_could_not_initialise_input_manager     = 0x0708;
	public const uint ERROR_clientlibrary_not_initialised          = 0x0709;
	public const uint ERROR_serverlibrary_not_initialised          = 0x070a;
	public const uint ERROR_whisper_too_many_targets               = 0x070b;
	public const uint ERROR_whisper_no_targets                     = 0x070c;
	
	//file transfer
	public const uint ERROR_file_invalid_name                      = 0x0800;
	public const uint ERROR_file_invalid_permissions               = 0x0801;
	public const uint ERROR_file_already_exists                    = 0x0802;
	public const uint ERROR_file_not_found                         = 0x0803;
	public const uint ERROR_file_io_error                          = 0x0804;
	public const uint ERROR_file_invalid_transfer_id               = 0x0805;
	public const uint ERROR_file_invalid_path                      = 0x0806;
	public const uint ERROR_file_no_files_available                = 0x0807;
	public const uint ERROR_file_overwrite_excludes_resume         = 0x0808;
	public const uint ERROR_file_invalid_size                      = 0x0809;
	public const uint ERROR_file_already_in_use                    = 0x080a;
	public const uint ERROR_file_could_not_open_connection         = 0x080b;
	public const uint ERROR_file_no_space_left_on_device           = 0x080c;
	public const uint ERROR_file_exceeds_file_system_maximum_size  = 0x080d;
	public const uint ERROR_file_transfer_connection_timeout       = 0x080e;
	public const uint ERROR_file_connection_lost                   = 0x080f;
	public const uint ERROR_file_exceeds_supplied_size             = 0x0810;
	public const uint ERROR_file_transfer_complete                 = 0x0811;
	public const uint ERROR_file_transfer_canceled                 = 0x0812;
	public const uint ERROR_file_transfer_interrupted              = 0x0813;
	public const uint ERROR_file_transfer_server_quota_exceeded    = 0x0814;
	public const uint ERROR_file_transfer_client_quota_exceeded    = 0x0815;
	public const uint ERROR_file_transfer_reset                    = 0x0816;
	public const uint ERROR_file_transfer_limit_reached            = 0x0817;
	
	//sound
	public const uint ERROR_sound_preprocessor_disabled            = 0x0900;
	public const uint ERROR_sound_internal_preprocessor            = 0x0901;
	public const uint ERROR_sound_internal_encoder                 = 0x0902;
	public const uint ERROR_sound_internal_playback                = 0x0903;
	public const uint ERROR_sound_no_capture_device_available      = 0x0904;
	public const uint ERROR_sound_no_playback_device_available     = 0x0905;
	public const uint ERROR_sound_could_not_open_capture_device    = 0x0906;
	public const uint ERROR_sound_could_not_open_playback_device   = 0x0907;
	public const uint ERROR_sound_handler_has_device               = 0x0908;
	public const uint ERROR_sound_invalid_capture_device           = 0x0909;
	public const uint ERROR_sound_invalid_playback_device          = 0x090a;
	public const uint ERROR_sound_invalid_wave                     = 0x090b;
	public const uint ERROR_sound_unsupported_wave                 = 0x090c;
	public const uint ERROR_sound_open_wave                        = 0x090d;
	public const uint ERROR_sound_internal_capture                 = 0x090e;
	public const uint ERROR_sound_device_in_use                    = 0x090f;
	public const uint ERROR_sound_device_already_registerred       = 0x0910;
	public const uint ERROR_sound_unknown_device                   = 0x0911;
	public const uint ERROR_sound_unsupported_frequency            = 0x0912;
	public const uint ERROR_sound_invalid_channel_count            = 0x0913;
	public const uint ERROR_sound_read_wave                        = 0x0914;
	public const uint ERROR_sound_need_more_data                   = 0x0915; //for internal purposes only
	public const uint ERROR_sound_device_busy                      = 0x0916; //for internal purposes only
	public const uint ERROR_sound_no_data                          = 0x0917;
	public const uint ERROR_sound_channel_mask_mismatch            = 0x0918;
	
	
	//permissions
	public const uint ERROR_permissions_client_insufficient        = 0x0a08;
	public const uint ERROR_permissions                            = 0x0a0c;
	
	//accounting
	public const uint ERROR_accounting_virtualserver_limit_reached = 0x0b00;
	public const uint ERROR_accounting_slot_limit_reached          = 0x0b01;
	public const uint ERROR_accounting_license_file_not_found      = 0x0b02;
	public const uint ERROR_accounting_license_date_not_ok         = 0x0b03;
	public const uint ERROR_accounting_unable_to_connect_to_server = 0x0b04;
	public const uint ERROR_accounting_unknown_error               = 0x0b05;
	public const uint ERROR_accounting_server_error                = 0x0b06;
	public const uint ERROR_accounting_instance_limit_reached      = 0x0b07;
	public const uint ERROR_accounting_instance_check_error        = 0x0b08;
	public const uint ERROR_accounting_license_file_invalid        = 0x0b09;
	public const uint ERROR_accounting_running_elsewhere           = 0x0b0a;
	public const uint ERROR_accounting_instance_duplicated         = 0x0b0b;
	public const uint ERROR_accounting_already_started             = 0x0b0c;
	public const uint ERROR_accounting_not_started                 = 0x0b0d;
	public const uint ERROR_accounting_to_many_starts              = 0x0b0e;
	
	//provisioning server
	public const uint ERROR_provisioning_invalid_password          = 0x1100;
	public const uint ERROR_provisioning_invalid_request           = 0x1101;
	public const uint ERROR_provisioning_no_slots_available        = 0x1102;
	public const uint ERROR_provisioning_pool_missing              = 0x1103;
	public const uint ERROR_provisioning_pool_unknown              = 0x1104;
	public const uint ERROR_provisioning_unknown_ip_location       = 0x1105;
	public const uint ERROR_provisioning_internal_tries_exceeded   = 0x1106;
	public const uint ERROR_provisioning_too_many_slots_requested  = 0x1107;
	public const uint ERROR_provisioning_too_many_reserved         = 0x1108;
	public const uint ERROR_provisioning_could_not_connect         = 0x1109;
	public const uint ERROR_provisioning_auth_server_not_connected = 0x1110;
	public const uint ERROR_provisioning_auth_data_too_large       = 0x1111;
	public const uint ERROR_provisioning_already_initialized       = 0x1112;
	public const uint ERROR_provisioning_not_initialized           = 0x1113;
	public const uint ERROR_provisioning_connecting                = 0x1114;
	public const uint ERROR_provisioning_already_connected         = 0x1115;
	public const uint ERROR_provisioning_not_connected             = 0x1116;
	public const uint ERROR_provisioning_io_error                  = 0x1117;
	public const uint ERROR_provisioning_invalid_timeout           = 0x1118;
	public const uint ERROR_provisioning_ts3server_not_found       = 0x1119;
	public const uint ERROR_provisioning_no_permission             = 0x111A;
}
