import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.UUID;

/**
 * A simple {@link Tuple} implementation using a {@link UUID} as the first element and a
 * {@link String} as second.
 * Due to JSON serialization a more abstract implementation could not be made. In tested cases
 * this resulted in a wrongly parsed {@link UUID}.
 */
public class UuidStringTuple implements Tuple<UUID, String> {

    @JsonProperty("item2")
    private UUID first;
    @JsonProperty("item1")
    private String second;

    public UuidStringTuple() {

    }

    public UuidStringTuple(UUID first, String second) {

        this.first = first;
        this.second = second;
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public UUID first() {

        return first;
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public String second() {

        return second;
    }

    public void setFirst(UUID first) {

        this.first = first;
    }

    public void setSecond(String second) {

        this.second = second;
    }
}
