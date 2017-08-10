import org.apache.kafka.common.serialization.Serializer;

import java.nio.charset.Charset;
import java.util.Map;

public class Tuple2Serializer implements Serializer<Tuple2> {

    @Override
    public void configure(Map<String, ?> map, boolean b) {
    }

    @Override
    public byte[] serialize(String s, Tuple2 tuple) {
        byte[] bytes = null;
        if (tuple != null){
            bytes = String.format("%d\t%d", tuple.count, tuple.timestamp).getBytes(Charset.forName("UTF-8"));
        }
        return bytes;
    }

    @Override
    public void close() {
    }
}