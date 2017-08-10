import org.apache.kafka.common.serialization.Deserializer;
import org.apache.kafka.common.serialization.Serializer;

import java.io.Closeable;
import java.nio.charset.Charset;
import java.util.Map;

public class Tuple2
{
    public Tuple2(Integer _count, Long _timestamp){
        timestamp = _timestamp;
        count = _count;
    }
    public Long timestamp;
    public Integer count;
}

