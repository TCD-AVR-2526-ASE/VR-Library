package edu.tcd.library.admin.service;

import edu.tcd.library.admin.dto.UnityCustomInfo;

public interface UnityService {

    String exchange();

    UnityCustomInfo customInfo(String accessToken, String customPlayerId);

    UnityCustomInfo customToken(String customPlayerId);
}
