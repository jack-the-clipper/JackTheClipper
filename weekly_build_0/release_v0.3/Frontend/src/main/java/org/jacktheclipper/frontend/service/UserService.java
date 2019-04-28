package org.jacktheclipper.frontend.service;

import org.apache.commons.lang3.NotImplementedException;
import org.springframework.stereotype.Service;

import java.util.UUID;

@Service
public class UserService {

    public void registerUser() {

        throw new NotImplementedException("UserService#registerUser is not implemented yet");
    }

    public void promoteUser() {
        throw new NotImplementedException("UserService#promoteUser is not implemented yet");
    }

    public void unlockUser() {
        throw new NotImplementedException("UserService#unlockUser is not implemented yet");
    }

    public void deleteUser() {
        throw new NotImplementedException("UserService#deleteUser is not implemented yet");
    }

    public void resetPassword(String eMail) {
        throw new NotImplementedException("UserService#resetPassword is not implemented yet");
    }

    public void updatePassword(UUID userId, String newPassword) {
        throw new NotImplementedException("UserService#updatePassword is not implemented yet");
    }


}
