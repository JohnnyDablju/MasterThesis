import org.apache.kafka.common.serialization.Deserializer;

import java.nio.charset.Charset;
import java.util.Map;

public class Tuple2Deserializer implements Deserializer<Tuple2> {

    @Override
    public void configure(Map<String, ?> map, boolean b) {
    }

    @Override
    public Tuple2 deserialize(String topic, byte[] bytes) {
        Tuple2 tuple = null;
        if (bytes != null){
            String[] parts = new String(bytes, Charset.forName("UTF-8")).split("\t");
            tuple = new Tuple2(Integer.parseInt(parts[0]), Long.parseLong(parts[1]));
        }
        return tuple;

    }

    @Override
    public void close() {
    }
}
