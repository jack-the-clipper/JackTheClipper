import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.List;
import java.util.UUID;

/**
 * An implementation of {@link Tuple} that represents a slimmer version of
 * {@link ExtendedUser}. It holds the {@link User} himself and only the ids of the
 * {@link OrganizationalUnit}s that he belongs to instead of the complete objects like
 * {@link ExtendedUser}.
 * See the documentation of
 * {@link UuidStringTuple} on why no more abstract {@link Tuple} implementation can be used
 */
public class UserUuidsTuple implements Tuple<User, List<UUID>> {

    @JsonProperty("item1")
    private User first;
    @JsonProperty("item2")
    private List<UUID> second;

    public UserUuidsTuple() {

    }

    public UserUuidsTuple(User first, List<UUID> second) {

        this.first = first;
        this.second = second;
    }

    /**
     * Returns the first element of the tuple
     *
     * @return The first element of the tuple
     */
    @Override
    public User first() {

        return first;
    }

    /**
     * Returns the second element of the tuple
     *
     * @return The second element of the tuple
     */
    @Override
    public List<UUID> second() {

        return second;
    }

    public User getFirst() {

        return first;
    }

    public void setFirst(User first) {

        this.first = first;
    }

    public List<UUID> getSecond() {

        return second;
    }

    public void setSecond(List<UUID> second) {

        this.second = second;
    }
}
