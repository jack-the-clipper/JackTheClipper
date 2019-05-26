/**
 * The abstract notion of a pair of two types. It is a sort of variation on
 * {@link java.util.Map.Entry} but lives outside of maps.
 *
 * @param <E> Class of the first field of this Tuple
 * @param <Y> Class of the second field of this Tuple
 */
public interface Tuple<E, Y> {

    /**
     * Returns the first element of the tuple
     *
     * @return The first element of the tuple
     */
    E first();

    /**
     * Returns the second element of the tuple
     *
     * @return The second element of the tuple
     */
    Y second();
}
